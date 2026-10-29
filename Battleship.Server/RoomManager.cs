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
/// Quản lý vòng đời của Server TCP: Lắng nghe kết nối, xử lý Handshake, 
/// duy trì Session của Client và định tuyến các gói tin (Packet).
/// </summary>
public class TcpServer
{
    private readonly TcpListener _listener;

    // Quản lý danh sách các session đang online (Thread-safe chống xung đột luồng)
    private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();

    // Quản lý phòng chơi (Phần của Đức đã tích hợp sẵn)
    private readonly RoomManager _roomManager = new();

    // Bộ định tuyến để chuyển tiếp gói tin đến đúng Handler xử lý
    private readonly PacketRouter _router;

    // Quản lý việc dừng Server an toàn bằng Token
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Khởi tạo Server TCP với cổng chỉ định
    /// </summary>
    public TcpServer(int port)
    {
        // Lắng nghe trên mọi giao diện mạng (IPv4) tại port được cấu hình
        _listener = new TcpListener(IPAddress.Any, port);
        _router = new PacketRouter(_sessions);
        RegisterHandlers();
    }

    // ===== ĐĂNG KÝ HANDLER: mỗi người THÊM 1 DÒNG cho mảng của mình, không sửa chỗ khác =====
    /// <summary>
    /// Đăng ký các bộ xử lý gói tin (Handlers) tương ứng với từng loại PacketType
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
    /// Khởi chạy Server và bắt đầu vòng lặp chấp nhận kết nối từ Client
    /// </summary>
    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"[TcpServer] Đang lắng nghe tại cổng {((IPEndPoint)_listener.LocalEndpoint).Port}...");

        try
        {
            // Vòng lặp vô hạn chấp nhận Client cho đến khi có tín hiệu dừng (Cancel)
            while (!_cts.IsCancellationRequested)
            {
                // Chờ đợi một Client kết nối tới (Bất đồng bộ)
                TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);

                // Sử dụng Discard (_) để chạy tác vụ xử lý Client độc lập, không block luồng chính
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[TcpServer] Đã dừng lắng nghe.");
        }
    }

    /// <summary>
    /// Xử lý quy trình kết nối ban đầu, xác thực và nhận dữ liệu từ Client
    /// </summary>
    private async Task HandleClientAsync(TcpClient client)
    {
        // Khởi tạo một phiên làm việc mới cho Client
        var session = new ClientSession(client);

        // Đăng ký sự kiện khi mất kết nối và khi nhận được gói tin từ Client
        session.OnDisconnected += OnSessionDisconnected;
        session.OnPacketReceived += _router.RouteAsync;

        // Tiến hành bắt tay (Handshake) để kiểm tra tính hợp lệ của Client
        bool accepted = await session.PerformHandshakeAsync(_cts.Token);
        if (!accepted)
        {
            Console.WriteLine($"[TcpServer] Handshake thất bại -> đóng session {session.SessionId}");
            session.Dispose();
            return;
        }

        // Thêm session vào danh sách quản lý nếu Handshake thành công
        _sessions.TryAdd(session.SessionId, session);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã kết nối. Online: {_sessions.Count}");

        // Bắt đầu vòng lặp nhận gói tin liên tục từ Client này
        await session.StartReceivingAsync(_cts.Token);
    }

    /// <summary>
    /// Xử lý dọn dẹp dữ liệu và thông báo cho người chơi khác khi một Client ngắt kết nối
    /// </summary>
    private void OnSessionDisconnected(ClientSession session)
    {
        // Xóa Session khỏi danh sách đang hoạt động
        _sessions.TryRemove(session.SessionId, out _);
        OnlineUsers.Remove(session);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã ngắt. Online: {_sessions.Count}");

        // Nếu Client này trước đó đã đăng nhập thành công, phát thông báo Offline cho toàn server
        if (session.IsLoggedIn && !string.IsNullOrEmpty(session.Username))
        {
            var offlinePacket = new Packet
            {
                Type = PacketType.OnlineListUpdate,
                Payload = JsonSerializer.Serialize(new { Username = session.Username, Status = "Offline" })
            };

            // Phát thông báo bất đồng bộ (Fire-and-forget) tới tất cả các Client khác
            _ = _router.BroadcastAsync(offlinePacket);
        }
    }

    /// <summary>
    /// Ngắt toàn bộ kết nối và dừng hoạt động của Server
    /// </summary>
    public void Stop()
    {
        _cts.Cancel(); // Kích hoạt hủy bỏ để dừng các vòng lặp while/await đang chạy
        _listener.Stop(); // Dừng lắng nghe cổng TCP
    }
}
