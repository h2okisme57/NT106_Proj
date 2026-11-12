using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Hàng đợi tìm trận (Matchmaking).
// Ai bấm "Tìm trận" thì bị bỏ vào hàng đợi, có 1 task chạy nền
// cứ lấy 2 người ở đầu hàng ra ghép thành 1 phòng.
public class MatchmakingQueue
{
    // hàng đợi chứa những người đang chờ (vào trước ra trước)
    // mỗi lần bấm "Tìm trận" có 1 "vé" (ticket) riêng đi kèm
    private readonly ConcurrentQueue<(ClientSession Session, string Ticket)> _queue =
        new ConcurrentQueue<(ClientSession Session, string Ticket)>();

    // id người chơi -> vé hiện hành của người đó (chỉ những người VẪN đang chờ).
    // ConcurrentQueue không xóa người ở giữa được nên khi hủy tìm trận mình chỉ xóa khỏi bảng này.
    // Dùng vé để phân biệt lần bấm cũ và mới: hủy rồi bấm lại thì trong hàng đợi có 2 mục của
    // cùng 1 người, nhưng chỉ mục có vé khớp bảng này mới hợp lệ (tránh ghép người đó với chính mình).
    private readonly ConcurrentDictionary<string, string> _waiting = new ConcurrentDictionary<string, string>();

    private readonly RoomManager _roomManager;

    public MatchmakingQueue(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    // Người chơi bấm "Tìm trận"
    public void Join(ClientSession session)
    {
        // nếu đã đang chờ rồi thì thôi, tránh bị vào 2 lần
        string ticket = Guid.NewGuid().ToString("N");
        if (!_waiting.TryAdd(session.SessionId, ticket))
        {
            return;
        }

        _queue.Enqueue((session, ticket));
        Console.WriteLine("[Matchmaking] " + session.SessionId + " vào hàng đợi. Đang chờ: " + _waiting.Count);
    }

    // Người chơi bấm hủy tìm trận (hoặc bị mất kết nối)
    public void Cancel(ClientSession session)
    {
        if (_waiting.TryRemove(session.SessionId, out _))
        {
            Console.WriteLine("[Matchmaking] " + session.SessionId + " rời hàng đợi.");
        }
    }

    // Task chạy nền, gọi 1 lần lúc server khởi động
    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine("[Matchmaking] Bắt đầu chạy worker ghép cặp...");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // chưa đủ 2 người thì nghỉ chút rồi kiểm tra lại
                if (_queue.Count < 2)
                {
                    await Task.Delay(500, ct);
                    continue;
                }

                (ClientSession Session, string Ticket) e1;
                (ClientSession Session, string Ticket) e2;
                if (!_queue.TryDequeue(out e1) || !_queue.TryDequeue(out e2))
                {
                    continue;
                }

                // kiểm tra cả 2 còn hợp lệ không (có thể 1 người đã hủy, bấm lại, hoặc tắt app lúc đang chờ)
                bool ok1 = IsStillOk(e1);
                bool ok2 = IsStillOk(e2);

                // không bao giờ ghép 1 người với chính mình
                if (ok1 && ok2 && e1.Session.SessionId == e2.Session.SessionId)
                {
                    ok2 = false;
                }

                if (ok1 && ok2)
                {
                    await CreateMatch(e1, e2);
                }
                else
                {
                    // người nào còn ok thì cho xếp hàng lại (giữ nguyên vé), người lỗi hoặc vé cũ thì bỏ luôn
                    if (ok1) _queue.Enqueue(e1);
                    if (ok2) _queue.Enqueue(e2);
                }
            }
            catch (OperationCanceledException)
            {
                // server tắt thì thoát vòng lặp, không phải lỗi gì
                break;
            }
            catch (Exception ex)
            {
                // có lỗi gì thì in ra chứ không cho worker chết
                Console.WriteLine("[Matchmaking] Lỗi: " + ex.Message);
            }
        }
    }

    // Kiểm tra 1 người còn hợp lệ để ghép không
    private bool IsStillOk((ClientSession Session, string Ticket) entry)
    {
        ClientSession s = entry.Session;
        if (s == null) return false;

        // đã hủy, hoặc đây là lần bấm cũ (vé không còn khớp vé hiện hành)
        if (!_waiting.TryGetValue(s.SessionId, out string? current) || current != entry.Ticket) return false;

        // socket đã đóng rồi thì không ghép (tránh lỗi null khi tạo trận)
        if (!s.Client.Connected)
        {
            _waiting.TryRemove(s.SessionId, out _);
            return false;
        }

        return true;
    }

    // Tạo phòng cho 2 người rồi báo cho cả 2
    private async Task CreateMatch((ClientSession Session, string Ticket) e1, (ClientSession Session, string Ticket) e2)
    {
        ClientSession p1 = e1.Session;
        ClientSession p2 = e2.Session;
        // đặt tên phòng ngẫu nhiên, lấy 6 ký tự đầu của Guid cho ngắn
        string roomId = "match-" + Guid.NewGuid().ToString("N").Substring(0, 6);

        GameRoom? room = _roomManager.CreateRoom(roomId, p1);
        if (room == null || !_roomManager.JoinRoom(roomId, p2))
        {
            // tạo phòng lỗi thì cho 2 người xếp hàng lại để thử lần sau
            Console.WriteLine("[Matchmaking] Tạo phòng " + roomId + " thất bại, xếp hàng lại.");
            _queue.Enqueue(e1);
            _queue.Enqueue(e2);
            return;
        }

        // ghép xong rồi thì xóa khỏi danh sách chờ
        _waiting.TryRemove(p1.SessionId, out _);
        _waiting.TryRemove(p2.SessionId, out _);

        Console.WriteLine("[Matchmaking] Ghép được " + p1.SessionId + " với " + p2.SessionId + " vào phòng " + roomId);

        // gửi thông báo cho từng người (mỗi người thấy tên của đối thủ)
        await SendMatchFound(p1, roomId, GetName(p2), true);
        await SendMatchFound(p2, roomId, GetName(p1), false);

        // báo luôn thông tin phòng để màn hình phòng chờ của cả 2 hiện đúng tên
        var roomInfo = new RoomUpdateDto
        {
            Room = new RoomInfoDto
            {
                RoomId = roomId,
                RoomName = roomId,
                HostName = GetName(p1),
                GuestName = GetName(p2),
                State = Battleship.Shared.RoomState.Waiting
            }
        };
        await p1.SendAsync(Packet.Create(PacketType.RoomUpdate, roomInfo));
        await p2.SendAsync(Packet.Create(PacketType.RoomUpdate, roomInfo));
    }

    private async Task SendMatchFound(ClientSession to, string roomId, string opponentName, bool isHost)
    {
        var dto = new MatchFoundDto
        {
            RoomId = roomId,
            OpponentName = opponentName,
            IsHost = isHost
        };

        try
        {
            await to.SendAsync(Packet.Create(PacketType.MatchFound, dto));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Matchmaking] Không gửi được MatchFound cho " + to.SessionId + ": " + ex.Message);
        }
    }

    // Lấy tên hiển thị, chưa đăng nhập thì dùng tạm 4 ký tự đầu của SessionId
    private string GetName(ClientSession s)
    {
        if (!string.IsNullOrEmpty(s.Username))
        {
            return s.Username;
        }
        return "Player-" + s.SessionId.Substring(0, 4);
    }
}
