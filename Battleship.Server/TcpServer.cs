using Battleship.Server.Handlers;
using Battleship.Shared;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Battleship.Server;

/// <summary>
/// Quản lý máy chủ TCP: lắng nghe kết nối, duy trì các phiên làm việc (session) của client,
/// thực hiện bắt tay (handshake) và định tuyến các gói tin đến bộ xử lý tương ứng.
/// </summary>
public class TcpServer
{
    private readonly TcpListener _listener;

    // Lưu trữ danh sách các session đang kết nối, sử dụng ConcurrentDictionary để đảm bảo thread-safe
    private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();

    // Quản lý các phòng chơi trong game (phần code của Đức)
    private readonly RoomManager _roomManager = new();

    // Bộ định tuyến gói tin dựa trên loại packet nhận được
    private readonly PacketRouter _router;

    // Quản lý việc hủy bỏ và dừng các tác vụ bất đồng bộ một cách an toàn
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Khởi tạo server TCP với cổng (port) được chỉ định.
    /// </summary>
    public TcpServer(int port)
    {
        // Lắng nghe kết nối từ bất kỳ địa chỉ IP nào tới cổng được cấu hình
        _listener = new TcpListener(IPAddress.Any, port);
        _router = new PacketRouter(_sessions);
        RegisterHandlers();
    }

    // ===== ĐĂNG KÝ HANDLER: mỗi người THÊM 1 DÒNG cho mảng của mình, không sửa chỗ khác =====
    /// <summary>
    /// Đăng ký các bộ xử lý gói tin (Handlers) với bộ định tuyến (Router).
    /// </summary>
    private void RegisterHandlers()
    {
        _router.Register(PacketType.ChatMessage, new ChatHandler(_router));   // Hoàng - chat sảnh

        // DONE (Dũng - 3.2): đăng ký AuthHandler cho LoginRequest / RegisterRequest
        var authHandler = new AuthHandler();
        _router.Register(PacketType.LoginRequest, authHandler);
        _router.Register(PacketType.RegisterRequest, authHandler);

        // ĐĂNG KÝ THÊM HANDLER XỬ LÝ BẠN BÈ (FriendHandler)
        var friendHandler = new FriendHandler(_sessions);
        _router.Register(PacketType.GetFriendsRequest, friendHandler);

        // DONE (Đức - 3.4): đăng ký RoomHandler (đã xoá đoạn khai báo trùng)
        var roomHandler = new RoomHandler(_roomManager, _router);
        _router.Register(PacketType.GetRoomListRequest, roomHandler);
        _router.Register(PacketType.CreateRoomRequest, roomHandler);
        _router.Register(PacketType.JoinRoomRequest, roomHandler);
        _router.Register(PacketType.LeaveRoom, roomHandler);
    }

    /// <summary>
    /// Bắt đầu lắng nghe và chấp nhận các kết nối đến từ client một cách bất đồng bộ.
    /// </summary>
    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"[TcpServer] Đang lắng nghe tại cổng {((IPEndPoint)_listener.LocalEndpoint).Port}...");

        try
        {
            // Tiếp tục vòng lặp chấp nhận kết nối cho đến khi có yêu cầu dừng server
            while (!_cts.IsCancellationRequested)
            {
                // Chờ và chấp nhận một kết nối TCP từ client mới
                TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);

                // Khởi chạy luồng xử lý client độc lập (fire-and-forget) để không làm nghẽn vòng lặp chấp nhận kết nối
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[TcpServer] Đã dừng lắng nghe.");
        }
    }

    /// <summary>
    /// Khởi tạo session, thực hiện quy trình bắt tay (handshake) và bắt đầu nhận dữ liệu từ client.
    /// </summary>
    private async Task HandleClientAsync(TcpClient client)
    {
        var session = new ClientSession(client);

        // Đăng ký sự kiện khi client ngắt kết nối và khi nhận được gói tin
        session.OnDisconnected += OnSessionDisconnected;
        session.OnPacketReceived += _router.RouteAsync;

        // Thực hiện quy trình bắt tay ban đầu để xác thực kết nối hợp lệ
        bool accepted = await session.PerformHandshakeAsync(_cts.Token);
        if (!accepted)
        {
            Console.WriteLine($"[TcpServer] Handshake thất bại -> đóng session {session.SessionId}");
            session.Dispose();
            return;
        }

        // Thêm session vào danh sách quản lý tập trung sau khi handshake thành công
        _sessions.TryAdd(session.SessionId, session);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã kết nối. Online: {_sessions.Count}");

        // Bắt đầu quy trình tiếp nhận và đọc các gói tin liên tục từ client này
        await session.StartReceivingAsync(_cts.Token);
    }

    /// <summary>
    /// Xử lý dọn dẹp dữ liệu khi một client ngắt kết nối và thông báo trạng thái offline cho các user khác.
    /// </summary>
    private void OnSessionDisconnected(ClientSession session)
    {
        // Loại bỏ session khỏi danh sách quản lý đang hoạt động
        _sessions.TryRemove(session.SessionId, out _);
        OnlineUsers.Remove(session);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã ngắt. Online: {_sessions.Count}");

        // Nếu client đã đăng nhập trước khi ngắt kết nối, gửi thông báo trạng thái "Offline" tới toàn bộ server
        if (session.IsLoggedIn && !string.IsNullOrEmpty(session.Username))
        {
            var offlinePacket = new Packet
            {
                Type = PacketType.OnlineListUpdate,
                Payload = JsonSerializer.Serialize(new { Username = session.Username, Status = "Offline" })
            };

            // Phát thông báo bất đồng bộ tới tất cả mọi người chơi khác đang online
            _ = _router.BroadcastAsync(offlinePacket);
        }
    }

    /// <summary>
    /// Kích hoạt hủy token và dừng bộ lắng nghe TCP để đóng server một cách an toàn.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}
