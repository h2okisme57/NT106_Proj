
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server
{
    // ============================================================
    // TASK 2.1: STATE MACHINE - QUẢN LÝ TRẠNG THÁI PHÒNG
    // ============================================================

    public enum RoomState
    {
        WaitingForPlayers, // Chờ đủ 2 người chơi.
        PlacementPhase,    // Hai người chơi đang xếp tàu.
        ReadyCheck,        // Chờ người chơi xác nhận sẵn sàng.
        TurnBasedPlay,     // Đang diễn ra trận đấu theo lượt.
        GameOver           // Trận đấu đã kết thúc.
    }

    // ============================================================
    // CLASS GAMEROOM: QUẢN LÝ THÔNG TIN MỘT PHÒNG CHƠI
    // ============================================================

    public class GameRoom
    {
        // ID định danh phòng chơi.
        public string RoomId { get; private set; }

        // Người tạo phòng, giữ quyền Host.
        public ClientSession Host { get; private set; }

        // Người chơi thứ hai, null khi phòng chưa đủ người.
        public ClientSession? Player2 { get; private set; }

        // Trạng thái hiện tại của phòng.
        public RoomState State { get; set; } =
            RoomState.WaitingForPlayers;

        // ========================================================
        // MỤC 4.2: QUẢN LÝ BÀN CỜ VÀ LƯỢT ĐÁNH
        // ========================================================

        // Bàn cờ riêng của Host và Player2.
        public PlayerBoard HostBoard { get; private set; }
            = new PlayerBoard();

        public PlayerBoard Player2Board { get; private set; }
            = new PlayerBoard();

        // SessionId của người chơi đang có lượt đánh.
        public string? CurrentTurnSessionId { get; set; }

        // Khóa đồng bộ các thao tác liên quan đến dữ liệu phòng.
        public readonly object RoomLock = new object();

        // Kiểm tra phòng đã đủ hai người chơi hay chưa.
        public bool IsFull => Host != null && Player2 != null;

        // Khởi tạo phòng mới với người tạo phòng là Host.
        public GameRoom(string roomId, ClientSession host)
        {
            RoomId = roomId;
            Host = host;
        }

        // Thêm người chơi thứ hai vào phòng.
        // Khi đủ người, khởi tạo lại bàn cờ và chuyển sang giai đoạn xếp tàu.
        public void AddPlayer(ClientSession player)
        {
            if (Player2 == null)
            {
                Player2 = player;

                HostBoard = new PlayerBoard();
                Player2Board = new PlayerBoard();

                State = RoomState.PlacementPhase;
            }
        }

        // Xóa người chơi thứ hai khỏi phòng.
        // Dừng Timer và đặt lại dữ liệu trận đấu.
        // Phải gọi trong lock(RoomLock).
        public void RemovePlayer(ClientSession player)
        {
            if (Player2 != null &&
                Player2.SessionId == player.SessionId)
            {
                StopTurnTimer();

                Player2 = null;
                HostBoard = new PlayerBoard();
                Player2Board = new PlayerBoard();
                CurrentTurnSessionId = null;

                // Phòng quay lại trạng thái chờ người chơi.
                State = RoomState.WaitingForPlayers;
            }
        }

        // Lấy bàn cờ của người chơi dựa trên SessionId.
        public PlayerBoard? GetBoard(ClientSession session)
        {
            if (Host.SessionId == session.SessionId)
                return HostBoard;

            if (Player2 != null &&
                Player2.SessionId == session.SessionId)
                return Player2Board;

            return null;
        }

        // Lấy bàn cờ đối thủ để phục vụ chức năng tấn công.
        public PlayerBoard? GetOpponentBoard(ClientSession session)
        {
            if (Host.SessionId == session.SessionId)
                return Player2Board;

            if (Player2 != null &&
                Player2.SessionId == session.SessionId)
                return HostBoard;

            return null;
        }

        // Xác định người chơi đối thủ trong phòng.
        public ClientSession? GetOpponent(ClientSession session)
        {
            if (Host.SessionId == session.SessionId)
                return Player2;

            if (Player2 != null &&
                Player2.SessionId == session.SessionId)
                return Host;

            return null;
        }

        // ========================================================
        // MỤC 4.3: TIMER 30 GIÂY CHO MỖI LƯỢT ĐÁNH
        // ========================================================

        // Dùng để hủy Timer khi lượt đánh kết thúc hoặc phòng bị giải tán.
        private CancellationTokenSource? _turnTimerCts;

        // Phiên bản Timer hiện tại, dùng để nhận biết Timer cũ.
        private int _turnVersion;

        // Khởi động hoặc đặt lại Timer cho lượt đánh.
        // onTimeoutAsync là callback xử lý khi hết thời gian.
        // Mặc định mỗi lượt có tối đa 30 giây.
        // Phải gọi bên trong lock(RoomLock).
        public void StartOrResetTurnTimer(
            Func<GameRoom, Task> onTimeoutAsync,
            int seconds = 30)
        {
            // Hủy Timer của lượt trước.
            StopTurnTimer();

            // Tạo Timer mới.
            var cts = new CancellationTokenSource();
            _turnTimerCts = cts;

            var token = cts.Token;

            // Tăng phiên bản để phân biệt Timer mới và Timer cũ.
            int expectedVersion = ++_turnVersion;

            // Chạy Timer trên tác vụ nền để không chặn luồng TCP.
            _ = Task.Run(async () =>
            {
                try
                {
                    // Đếm ngược thời gian của lượt đánh.
                    await Task.Delay(
                        TimeSpan.FromSeconds(seconds), token);

                    // Kiểm tra Timer còn hợp lệ hay không.
                    lock (RoomLock)
                    {
                        if (token.IsCancellationRequested ||
                            expectedVersion != _turnVersion ||
                            State != RoomState.TurnBasedPlay)
                        {
                            return;
                        }
                    }

                    // Gọi xử lý khi người chơi hết thời gian.
                    await onTimeoutAsync(this);
                }
                catch (OperationCanceledException)
                {
                    // Timer bị hủy khi lượt đánh thay đổi.
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[TurnTimer] Lỗi phòng {RoomId}: {ex.Message}");
                }
            });
        }

        // Dừng Timer hiện tại.
        // Phải gọi bên trong lock(RoomLock).
        public void StopTurnTimer()
        {
            // Tăng phiên bản để vô hiệu hóa Timer cũ.
            _turnVersion++;

            var oldCts = _turnTimerCts;
            _turnTimerCts = null;

            if (oldCts != null)
            {
                try
                {
                    oldCts.Cancel();
                }
                finally
                {
                    oldCts.Dispose();
                }
            }
        }
    }

    // ============================================================
    // CLASS ROOMMANAGER: QUẢN LÝ DANH SÁCH PHÒNG CHƠI
    // ============================================================

    public class RoomManager
    {
        // ConcurrentDictionary hỗ trợ truy cập danh sách phòng
        // an toàn khi nhiều luồng cùng thao tác.
        private readonly ConcurrentDictionary<string, GameRoom>
            _rooms = new ConcurrentDictionary<string, GameRoom>();

        // ========================================================
        // MỤC 4.5: SỰ KIỆN KẾT THÚC TRẬN ĐẤU VÀ LƯU LỊCH SỬ
        // ========================================================

        // Cho phép các thành phần khác đăng ký xử lý khi trận đấu kết thúc.
        public event Func<GameOverDto, Task>? OnMatchFinished;

        // Phát sự kiện đến tất cả subscriber đã đăng ký.
        public async Task RaiseMatchFinishedAsync(
            GameOverDto gameOverDto)
        {
            var handler = OnMatchFinished;

            if (handler == null)
                return;

            // Xử lý từng subscriber riêng biệt.
            // Mỗi Task đều được await để đảm bảo hoàn thành.
            foreach (Func<GameOverDto, Task> subscriber
                     in handler.GetInvocationList())
            {
                try
                {
                    await subscriber(gameOverDto);
                }
                catch (Exception ex)
                {
                    // Một subscriber lỗi không ảnh hưởng subscriber khác.
                    Console.WriteLine(
                        $"[RoomManager] Lỗi ghi lịch sử: {ex.Message}");
                }
            }
        }

        // ========================================================
        // CÁC CHỨC NĂNG QUẢN LÝ PHÒNG
        // ========================================================

        // 1. TẠO PHÒNG MỚI
        // TryAdd giúp tránh trùng RoomId.
        public GameRoom? CreateRoom(
            string roomId,
            ClientSession host)
        {
            var newRoom = new GameRoom(roomId, host);

            if (_rooms.TryAdd(roomId, newRoom))
            {
                Console.WriteLine(
                    $"[RoomManager] Đã tạo phòng {roomId}. " +
                    $"Host: {host.SessionId}");

                return newRoom;
            }

            return null;
        }

        // 2. THAM GIA PHÒNG
        // Chỉ cho phép thêm người chơi khi phòng còn chỗ.
        public bool JoinRoom(
            string roomId,
            ClientSession player)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                lock (room.RoomLock)
                {
                    if (!room.IsFull &&
                        room.Host.SessionId != player.SessionId)
                    {
                        room.AddPlayer(player);

                        Console.WriteLine(
                            $"[RoomManager] {player.SessionId} " +
                            $"đã vào phòng {roomId}");

                        return true;
                    }
                }
            }

            return false;
        }

        // 3. KICK NGƯỜI CHƠI
        // Chỉ Host mới được quyền Kick Player2.
        public bool KickPlayer(
            string roomId,
            ClientSession requestClient,
            ClientSession targetPlayer)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                lock (room.RoomLock)
                {
                    if (room.Host.SessionId ==
                            requestClient.SessionId &&
                        room.Player2 != null &&
                        room.Player2.SessionId ==
                            targetPlayer.SessionId)
                    {
                        room.RemovePlayer(targetPlayer);
                        return true;
                    }
                }
            }

            return false;
        }

        // 4. LẤY THÔNG TIN PHÒNG THEO ROOMID.
        public GameRoom? GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        // 5. TÌM PHÒNG THEO NGƯỜI CHƠI.
        public GameRoom? FindRoomBySession(ClientSession session)
        {
            return _rooms.Values.FirstOrDefault(r =>
                r.Host.SessionId == session.SessionId ||
                (r.Player2 != null &&
                 r.Player2.SessionId == session.SessionId));
        }

        // 6. LẤY DANH SÁCH TẤT CẢ PHÒNG.
        public List<GameRoom> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }

        // 7. RỜI PHÒNG.
        // Host rời phòng: giải tán phòng.
        // Player2 rời phòng: giữ phòng và quay về trạng thái chờ.
        public bool LeaveRoom(
            string roomId,
            ClientSession session)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                lock (room.RoomLock)
                {
                    // Dừng Timer trước khi xử lý người chơi rời phòng.
                    room.StopTurnTimer();

                    // Nếu Host rời phòng thì xóa toàn bộ phòng.
                    if (room.Host.SessionId ==
                        session.SessionId)
                    {
                        _rooms.TryRemove(roomId, out _);

                        Console.WriteLine(
                            $"[RoomManager] Host rời phòng {roomId}");

                        return true;
                    }

                    // Nếu Player2 rời phòng thì chỉ xóa Player2.
                    if (room.Player2 != null &&
                        room.Player2.SessionId ==
                            session.SessionId)
                    {
                        room.RemovePlayer(session);

                        Console.WriteLine(
                            $"[RoomManager] Player2 rời phòng {roomId}");

                        return true;
                    }
                }
            }

            return false;
        }

        // 8. XÓA PHÒNG KHỎI HỆ THỐNG.
        // Dừng Timer để tránh tác vụ cũ tiếp tục xử lý.
        public void RemoveRoom(string roomId)
        {
            if (_rooms.TryRemove(roomId, out var room))
            {
                lock (room.RoomLock)
                {
                    room.StopTurnTimer();
                }
            }
        }
    }
}
