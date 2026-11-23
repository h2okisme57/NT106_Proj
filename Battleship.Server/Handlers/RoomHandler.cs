
using System;
using System.Linq;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server.Handlers
{
    public class RoomHandler : IPacketHandler
    {
        private readonly RoomManager _roomManager;
        private readonly PacketRouter _router;

        // Thời gian tối đa cho mỗi lượt đánh.
        public const int TurnTimeLimitSeconds = 30;

        public RoomHandler(RoomManager roomManager, PacketRouter router)
        {
            _roomManager = roomManager;
            _router = router;
        }

        // ============================================================
        // 1. ĐIỀU PHỐI GÓI TIN
        // ============================================================

        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.GetRoomListRequest:
                    await HandleGetRoomListAsync(session);
                    break;

                case PacketType.CreateRoomRequest:
                    await HandleCreateRoomAsync(session, packet);
                    break;

                case PacketType.JoinRoomRequest:
                    await HandleJoinRoomAsync(session, packet);
                    break;

                case PacketType.LeaveRoom:
                    await HandleLeaveRoomAsync(session, packet);
                    break;

                case PacketType.KickPlayerRequest:
                    await HandleKickPlayerAsync(session, packet);
                    break;

                case PacketType.PlaceShips:
                    await HandlePlaceShipsAsync(session, packet);
                    break;

                case PacketType.PlayerReady:
                    await HandlePlayerReadyAsync(session, packet);
                    break;

                case PacketType.FireShot:
                    await HandleFireShotAsync(session, packet);
                    break;

                default:
                    Console.WriteLine(
                        $"[RoomHandler] Không hỗ trợ gói tin: {packet.Type}");
                    break;
            }
        }

        // ============================================================
        // 2. LẤY DANH SÁCH PHÒNG
        // ============================================================

        private async Task HandleGetRoomListAsync(ClientSession session)
        {
            var rooms = _roomManager.GetAllRooms()
                .Select(ToRoomInfoDto)
                .ToList();

            await session.SendAsync(
                Packet.Create(PacketType.GetRoomListResponse,
                new RoomListResponseDto
                {
                    Rooms = rooms
                }));
        }

        // ============================================================
        // 3. TẠO PHÒNG
        // ============================================================

        private async Task HandleCreateRoomAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<CreateRoomDto>();

            string roomId =
                dto != null && !string.IsNullOrWhiteSpace(dto.RoomId)
                ? dto.RoomId
                : Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var room = _roomManager.CreateRoom(roomId, session);

            await session.SendAsync(
                Packet.Create(PacketType.RoomUpdate,
                new RoomUpdateDto
                {
                    Room = room != null ? ToRoomInfoDto(room) : null!
                }));
        }

        // ============================================================
        // 4. THAM GIA PHÒNG
        // ============================================================

        private async Task HandleJoinRoomAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<JoinRoomDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
            {
                await session.SendAsync(
                    Packet.Create(PacketType.JoinRoomResponse,
                    new JoinRoomResponseDto
                    {
                        Success = false,
                        Message = "Thiếu mã phòng."
                    }));
                return;
            }

            bool joined = _roomManager.JoinRoom(dto.RoomId, session);

            var room = joined
                ? _roomManager.GetRoom(dto.RoomId)
                : null;

            var updateDto = new RoomUpdateDto
            {
                Room = room != null ? ToRoomInfoDto(room) : null!
            };

            await session.SendAsync(
                Packet.Create(PacketType.JoinRoomResponse,
                new JoinRoomResponseDto
                {
                    Success = joined && room != null,
                    Message = joined && room != null
                        ? string.Empty
                        : "Không vào được phòng: phòng không tồn tại hoặc đã đầy.",
                    Room = updateDto.Room
                }));

            // Cập nhật phòng cho Host.
            if (joined && room != null)
            {
                await room.Host.SendAsync(
                    Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        // ============================================================
        // 5. RỜI PHÒNG
        // ============================================================

        private async Task HandleLeaveRoomAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<LeaveRoomDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            await ProcessPlayerLeaveOrDisconnectAsync(
                session, dto.RoomId);
        }

        // ============================================================
        // 6. KICK NGƯỜI CHƠI
        // ============================================================

        private async Task HandleKickPlayerAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<KickPlayerDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            var room = _roomManager.GetRoom(dto.RoomId);

            if (room == null || room.Player2 == null)
                return;

            var kickedPlayer = room.Player2;

            bool kicked = _roomManager.KickPlayer(
                dto.RoomId, session, kickedPlayer);

            if (!kicked)
                return;

            // Thông báo người bị Kick.
            await kickedPlayer.SendAsync(
                Packet.Create(PacketType.KickedFromRoom,
                new RoomUpdateDto { Room = null! }));

            // Cập nhật lại phòng cho Host.
            await room.Host.SendAsync(
                Packet.Create(PacketType.RoomUpdate,
                new RoomUpdateDto
                {
                    Room = ToRoomInfoDto(room)
                }));
        }

        // ============================================================
        // 7. ĐẶT TÀU
        // ============================================================

        private async Task HandlePlaceShipsAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<PlaceShipsDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            var room = _roomManager.GetRoom(dto.RoomId);

            if (room == null)
                return;

            bool placedOk = false;
            string errorMessage = string.Empty;

            lock (room.RoomLock)
            {
                var board = room.GetBoard(session);

                if (board != null &&
                    (room.State == RoomState.PlacementPhase ||
                     room.State == RoomState.ReadyCheck))
                {
                    placedOk = board.TryPlaceShips(
                        dto.Ships, out errorMessage);

                    if (placedOk)
                        board.IsReady = false;
                }
            }

            if (!placedOk)
            {
                Console.WriteLine(
                    $"[TurnEngine] Đặt tàu thất bại: {errorMessage}");
            }

            await Task.CompletedTask;
        }

        // ============================================================
        // 8. READY VÀ BẮT ĐẦU TRẬN ĐẤU
        // ============================================================

        private async Task HandlePlayerReadyAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<PlayerReadyDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            var room = _roomManager.GetRoom(dto.RoomId);

            if (room == null)
                return;

            bool startMatch = false;
            string firstTurnUsername = string.Empty;

            lock (room.RoomLock)
            {
                if (!room.IsFull || room.Player2 == null)
                    return;

                var board = room.GetBoard(session);

                if (board == null || board.Ships.Count == 0)
                {
                    Console.WriteLine(
                        $"[TurnEngine] {session.SessionId} chưa đặt tàu.");
                    return;
                }

                board.IsReady = dto.IsReady;

                room.State = RoomState.ReadyCheck;

                // Cả hai người chơi Ready thì bắt đầu trận đấu.
                if (room.HostBoard.IsReady &&
                    room.Player2Board.IsReady)
                {
                    room.State = RoomState.TurnBasedPlay;

                    // Host đánh lượt đầu tiên.
                    room.CurrentTurnSessionId = room.Host.SessionId;

                    firstTurnUsername = GetDisplayName(room.Host);
                    startMatch = true;

                    // Khởi động Timer 30 giây đầu tiên.
                    room.StartOrResetTurnTimer(
                        HandleTurnTimeoutAsync,
                        TurnTimeLimitSeconds);
                }
            }

            if (startMatch && room.Player2 != null)
            {
                var startPacket = Packet.Create(
                    PacketType.TurnChanged,
                    new TurnChangedDto
                    {
                        RoomId = room.RoomId,
                        CurrentTurnUsername = firstTurnUsername,
                        TimeLimitSeconds = TurnTimeLimitSeconds,
                        Reason = "GameStart"
                    });

                await room.Host.SendAsync(startPacket);
                await room.Player2.SendAsync(startPacket);

                Console.WriteLine(
                    $"[TurnEngine] Phòng {room.RoomId} bắt đầu. " +
                    $"Lượt đầu: {firstTurnUsername}");
            }
        }

        // ============================================================
        // 9. XỬ LÝ BẮN (FIRESHOT)
        // ============================================================

        private async Task HandleFireShotAsync(
            ClientSession session, Packet packet)
        {
            var dto = packet.DeserializePayload<FireShotDto>();

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoomId))
                return;

            var room = _roomManager.GetRoom(dto.RoomId);

            if (room == null)
                return;

            ShotResultDto? shotResult = null;
            TurnChangedDto? turnChanged = null;
            GameOverDto? gameOver = null;

            ClientSession? hostSession = null;
            ClientSession? player2Session = null;

            lock (room.RoomLock)
            {
                // Chỉ được bắn khi trận đấu đang diễn ra.
                if (room.State != RoomState.TurnBasedPlay ||
                    room.Player2 == null)
                    return;

                // Kiểm tra đúng lượt người chơi.
                if (room.CurrentTurnSessionId != session.SessionId)
                {
                    Console.WriteLine(
                        $"[TurnEngine] {session.SessionId} bắn sai lượt.");
                    return;
                }

                var opponent = room.GetOpponent(session);
                var opponentBoard = room.GetOpponentBoard(session);

                if (opponent == null || opponentBoard == null)
                    return;

                hostSession = room.Host;
                player2Session = room.Player2;

                // Xử lý phát bắn.
                var (isValid, isHit, isSunk, sunkShipName) =
                    opponentBoard.ProcessShot(dto.X, dto.Y);

                string shooterName = GetDisplayName(session);
                string opponentName = GetDisplayName(opponent);

                // ----------------------------------------------------
                // 9.1. BẮN KHÔNG HỢP LỆ
                // ----------------------------------------------------

                if (!isValid)
                {
                    shotResult = new ShotResultDto
                    {
                        RoomId = room.RoomId,
                        ShooterUsername = shooterName,
                        X = dto.X,
                        Y = dto.Y,
                        IsValid = false,
                        IsHit = false,
                        IsSunk = false,
                        NextTurnUsername = shooterName
                    };
                }

                // ----------------------------------------------------
                // 9.2. BẮN TRÚNG
                // ----------------------------------------------------

                else if (isHit)
                {
                    // Kiểm tra toàn bộ tàu đối phương đã chìm chưa.
                    if (opponentBoard.AllShipsSunk)
                    {
                        // Dừng Timer và kết thúc trận đấu.
                        room.StopTurnTimer();

                        room.State = RoomState.GameOver;
                        room.CurrentTurnSessionId = null;

                        shotResult = new ShotResultDto
                        {
                            RoomId = room.RoomId,
                            ShooterUsername = shooterName,
                            X = dto.X,
                            Y = dto.Y,
                            IsValid = true,
                            IsHit = true,
                            IsSunk = isSunk,
                            SunkShipName = sunkShipName,
                            NextTurnUsername = string.Empty
                        };

                        gameOver = new GameOverDto
                        {
                            RoomId = room.RoomId,
                            WinnerUsername = shooterName,
                            LoserUsername = opponentName,
                            Reason = "AllShipsSunk"
                        };
                    }
                    else
                    {
                        // Bắn trúng nhưng chưa chìm hết tàu:
                        // Giữ nguyên lượt cho người vừa bắn.
                        room.StartOrResetTurnTimer(
                            HandleTurnTimeoutAsync,
                            TurnTimeLimitSeconds);

                        shotResult = new ShotResultDto
                        {
                            RoomId = room.RoomId,
                            ShooterUsername = shooterName,
                            X = dto.X,
                            Y = dto.Y,
                            IsValid = true,
                            IsHit = true,
                            IsSunk = isSunk,
                            SunkShipName = sunkShipName,
                            NextTurnUsername = shooterName
                        };
                    }
                }

                // ----------------------------------------------------
                // 9.3. BẮN HỤT
                // ----------------------------------------------------

                else
                {
                    // Chuyển lượt sang đối thủ.
                    room.CurrentTurnSessionId = opponent.SessionId;

                    // Đặt lại Timer 30 giây.
                    room.StartOrResetTurnTimer(
                        HandleTurnTimeoutAsync,
                        TurnTimeLimitSeconds);

                    shotResult = new ShotResultDto
                    {
                        RoomId = room.RoomId,
                        ShooterUsername = shooterName,
                        X = dto.X,
                        Y = dto.Y,
                        IsValid = true,
                        IsHit = false,
                        IsSunk = false,
                        NextTurnUsername = opponentName
                    };

                    turnChanged = new TurnChangedDto
                    {
                        RoomId = room.RoomId,
                        CurrentTurnUsername = opponentName,
                        TimeLimitSeconds = TurnTimeLimitSeconds,
                        Reason = "Miss"
                    };
                }
            }

            // --------------------------------------------------------
            // 9.4. BÁO KẾT QUẢ BẮN KHÔNG HỢP LỆ
            // --------------------------------------------------------

            if (shotResult != null && !shotResult.IsValid)
            {
                await session.SendAsync(
                    Packet.Create(PacketType.ShotResult, shotResult));
                return;
            }

            // --------------------------------------------------------
            // 9.5. GỬI KẾT QUẢ BẮN CHO HAI NGƯỜI
            // --------------------------------------------------------

            if (shotResult != null &&
                hostSession != null &&
                player2Session != null)
            {
                var shotPacket = Packet.Create(
                    PacketType.ShotResult, shotResult);

                await hostSession.SendAsync(shotPacket);
                await player2Session.SendAsync(shotPacket);
            }

            // --------------------------------------------------------
            // 9.6. THÔNG BÁO ĐỔI LƯỢT DO BẮN HỤT
            // --------------------------------------------------------

            if (turnChanged != null &&
                hostSession != null &&
                player2Session != null)
            {
                var turnPacket = Packet.Create(
                    PacketType.TurnChanged, turnChanged);

                await hostSession.SendAsync(turnPacket);
                await player2Session.SendAsync(turnPacket);
            }

            // --------------------------------------------------------
            // 9.7. KẾT THÚC TRẬN ĐẤU
            // --------------------------------------------------------

            if (gameOver != null &&
                hostSession != null &&
                player2Session != null)
            {
                var overPacket = Packet.Create(
                    PacketType.GameOver, gameOver);

                await hostSession.SendAsync(overPacket);
                await player2Session.SendAsync(overPacket);

                Console.WriteLine(
                    $"[TurnEngine] Phòng {room.RoomId} kết thúc. " +
                    $"Người thắng: {gameOver.WinnerUsername}");

                // Gọi sự kiện lưu lịch sử trận đấu.
                await _roomManager.RaiseMatchFinishedAsync(gameOver);
            }
        }

        // ============================================================
        // 10. XỬ LÝ TIMEOUT 30 GIÂY
        // ============================================================

        private async Task HandleTurnTimeoutAsync(GameRoom room)
        {
            TurnChangedDto? turnChanged = null;

            ClientSession? hostSession = null;
            ClientSession? player2Session = null;

            lock (room.RoomLock)
            {
                if (room.State != RoomState.TurnBasedPlay ||
                    room.Player2 == null)
                    return;

                // Xác định người chơi tiếp theo.
                var nextPlayer =
                    room.CurrentTurnSessionId == room.Host.SessionId
                    ? room.Player2
                    : room.Host;

                room.CurrentTurnSessionId = nextPlayer.SessionId;

                string nextTurnName = GetDisplayName(nextPlayer);

                hostSession = room.Host;
                player2Session = room.Player2;

                turnChanged = new TurnChangedDto
                {
                    RoomId = room.RoomId,
                    CurrentTurnUsername = nextTurnName,
                    TimeLimitSeconds = TurnTimeLimitSeconds,
                    Reason = "Timeout"
                };

                // Khởi động Timer mới cho người tiếp theo.
                room.StartOrResetTurnTimer(
                    HandleTurnTimeoutAsync,
                    TurnTimeLimitSeconds);
            }

            if (turnChanged != null &&
                hostSession != null &&
                player2Session != null)
            {
                var timeoutPacket = Packet.Create(
                    PacketType.TurnChanged, turnChanged);

                await hostSession.SendAsync(timeoutPacket);
                await player2Session.SendAsync(timeoutPacket);

                Console.WriteLine(
                    $"[TurnTimer] Phòng {room.RoomId} hết 30 giây. " +
                    $"Chuyển lượt: {turnChanged.CurrentTurnUsername}");
            }
        }

        // ============================================================
        // 11. XỬ LÝ KHI CLIENT MẤT KẾT NỐI
        // ============================================================

        // Hàm này cần được gọi từ TCP Server khi Client disconnect.
        public async Task HandleSessionDisconnectedAsync(
            ClientSession session)
        {
            var room = _roomManager.FindRoomBySession(session);

            if (room != null)
            {
                await ProcessPlayerLeaveOrDisconnectAsync(
                    session, room.RoomId);
            }
        }

        // ============================================================
        // 12. XỬ LÝ NGƯỜI CHƠI RỜI PHÒNG HOẶC RỚT MẠNG
        // ============================================================

        private async Task ProcessPlayerLeaveOrDisconnectAsync(
            ClientSession session, string roomId)
        {
            var room = _roomManager.GetRoom(roomId);

            if (room == null)
                return;

            ClientSession? remainingPlayer = null;
            GameOverDto? forfeitGameOver = null;

            lock (room.RoomLock)
            {
                // Xác định người chơi còn lại.
                remainingPlayer = room.GetOpponent(session);

                // Nếu đang đánh mà một người rời đi:
                // Người còn lại được xử thắng.
                if (room.State == RoomState.TurnBasedPlay &&
                    remainingPlayer != null)
                {
                    room.StopTurnTimer();

                    room.State = RoomState.GameOver;
                    room.CurrentTurnSessionId = null;

                    forfeitGameOver = new GameOverDto
                    {
                        RoomId = room.RoomId,
                        WinnerUsername = GetDisplayName(remainingPlayer),
                        LoserUsername = GetDisplayName(session),
                        Reason = "OpponentDisconnected"
                    };
                }
            }

            // Xóa người chơi khỏi phòng.
            bool left = _roomManager.LeaveRoom(roomId, session);

            if (!left)
                return;

            // --------------------------------------------------------
            // 12.1. RỜI PHÒNG GIỮA TRẬN
            // --------------------------------------------------------

            if (forfeitGameOver != null &&
                remainingPlayer != null)
            {
                // Thông báo chiến thắng cho người còn lại.
                await remainingPlayer.SendAsync(
                    Packet.Create(PacketType.GameOver, forfeitGameOver));

                // Lưu lịch sử trận đấu.
                await _roomManager.RaiseMatchFinishedAsync(
                    forfeitGameOver);

                // Xóa phòng sau khi kết thúc trận đấu.
                _roomManager.RemoveRoom(roomId);

                Console.WriteLine(
                    $"[TurnEngine] Người chơi {forfeitGameOver.LoserUsername} " +
                    $"mất kết nối. Người thắng: " +
                    $"{forfeitGameOver.WinnerUsername}");

                return;
            }

            // --------------------------------------------------------
            // 12.2. RỜI PHÒNG KHI ĐANG CHỜ
            // --------------------------------------------------------

            if (remainingPlayer != null)
            {
                var updatedRoom = _roomManager.GetRoom(roomId);

                var updateDto = new RoomUpdateDto
                {
                    Room = updatedRoom != null
                        ? ToRoomInfoDto(updatedRoom)
                        : null!
                };

                await remainingPlayer.SendAsync(
                    Packet.Create(PacketType.RoomUpdate, updateDto));
            }
        }

        // ============================================================
        // 13. LẤY TÊN HIỂN THỊ
        // ============================================================

        private static string GetDisplayName(ClientSession session)
        {
            return !string.IsNullOrWhiteSpace(session.Username)
                ? session.Username
                : session.SessionId;
        }

        // ============================================================
        // 14. CHUYỂN GAMEROOM THÀNH ROOMINFODTO
        // ============================================================

        private static RoomInfoDto ToRoomInfoDto(GameRoom room)
        {
            return new RoomInfoDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomId,

                HostName = room.Host.Username ?? room.Host.SessionId,

                GuestName = room.Player2 != null
                    ? (room.Player2.Username ?? room.Player2.SessionId)
                    : null,

                State = room.State == RoomState.WaitingForPlayers
                    ? Battleship.Shared.RoomState.Waiting
                    : Battleship.Shared.RoomState.Playing
            };
        }
    }
}
