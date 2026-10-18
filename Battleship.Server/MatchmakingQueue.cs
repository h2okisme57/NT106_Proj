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
    private readonly ConcurrentQueue<ClientSession> _queue = new ConcurrentQueue<ClientSession>();

    // danh sách id của những người VẪN đang chờ.
    // ConcurrentQueue không xóa người ở giữa được nên khi người ta hủy tìm trận
    // mình chỉ xóa khỏi danh sách này, lúc lấy ra mà không thấy trong đây thì bỏ qua.
    private readonly ConcurrentDictionary<string, bool> _waiting = new ConcurrentDictionary<string, bool>();

    private readonly RoomManager _roomManager;

    public MatchmakingQueue(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    // Người chơi bấm "Tìm trận"
    public void Join(ClientSession session)
    {
        // nếu đã đang chờ rồi thì thôi, tránh bị vào 2 lần
        if (!_waiting.TryAdd(session.SessionId, true))
        {
            return;
        }

        _queue.Enqueue(session);
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

                ClientSession? p1;
                ClientSession? p2;
                _queue.TryDequeue(out p1);
                _queue.TryDequeue(out p2);

                // kiểm tra cả 2 còn sống không (có thể 1 người đã hủy hoặc tắt app lúc đang chờ)
                bool ok1 = IsStillOk(p1);
                bool ok2 = IsStillOk(p2);

                if (ok1 && ok2)
                {
                    await CreateMatch(p1!, p2!);
                }
                else
                {
                    // người nào còn ok thì cho xếp hàng lại, người lỗi thì bỏ luôn
                    if (ok1) _queue.Enqueue(p1!);
                    if (ok2) _queue.Enqueue(p2!);
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
    private bool IsStillOk(ClientSession? s)
    {
        if (s == null) return false;

        // đã hủy / đã bị xóa khỏi danh sách chờ
        if (!_waiting.ContainsKey(s.SessionId)) return false;

        // socket đã đóng rồi thì không ghép (tránh lỗi null khi tạo trận)
        if (!s.Client.Connected)
        {
            _waiting.TryRemove(s.SessionId, out _);
            return false;
        }

        return true;
    }

    // Tạo phòng cho 2 người rồi báo cho cả 2
    private async Task CreateMatch(ClientSession p1, ClientSession p2)
    {
        // đặt tên phòng ngẫu nhiên, lấy 6 ký tự đầu của Guid cho ngắn
        string roomId = "match-" + Guid.NewGuid().ToString("N").Substring(0, 6);

        GameRoom? room = _roomManager.CreateRoom(roomId, p1);
        if (room == null || !_roomManager.JoinRoom(roomId, p2))
        {
            // tạo phòng lỗi thì cho 2 người xếp hàng lại để thử lần sau
            Console.WriteLine("[Matchmaking] Tạo phòng " + roomId + " thất bại, xếp hàng lại.");
            _queue.Enqueue(p1);
            _queue.Enqueue(p2);
            return;
        }

        // ghép xong rồi thì xóa khỏi danh sách chờ
        _waiting.TryRemove(p1.SessionId, out _);
        _waiting.TryRemove(p2.SessionId, out _);

        Console.WriteLine("[Matchmaking] Ghép được " + p1.SessionId + " với " + p2.SessionId + " vào phòng " + roomId);

        // gửi thông báo cho từng người (mỗi người thấy tên của đối thủ)
        await SendMatchFound(p1, roomId, GetName(p2), true);
        await SendMatchFound(p2, roomId, GetName(p1), false);
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
