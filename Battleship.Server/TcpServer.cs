
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

// Lõi mạng Server: lắng nghe TcpListener bất đồng bộ,
// quản lý danh sách session online.
public class TcpServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();

    private readonly RoomManager _roomManager = new();
    private readonly PacketRouter _router;
    private readonly MatchmakingQueue _matchmaking;
    private readonly CancellationTokenSource _cts = new();

    // Giữ tham chiếu RoomHandler để xử lý khi Client disconnect.
    private RoomHandler? _roomHandler;

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _router = new PacketRouter(_sessions);
        _matchmaking = new MatchmakingQueue(_roomManager);

        RegisterHandlers();
    }

    // ============================================================
    // ĐĂNG KÝ HANDLERS
    // ============================================================

    private void RegisterHandlers()
    {
        // Hoàng - Chat sảnh
        _router.Register(
            PacketType.ChatMessage,
            new ChatHandler(_router));

        // Dũng - Authentication
        var authHandler = new AuthHandler(
            _router,
            s => _matchmaking.Cancel(s));

        _router.Register(PacketType.LoginRequest, authHandler);
        _router.Register(PacketType.RegisterRequest, authHandler);
        _router.Register(PacketType.LogoutRequest, authHandler);

        // Hoàng - Password Reset
        var passwordResetHandler = new PasswordResetHandler();

        _router.Register(
            PacketType.ForgotPasswordRequest,
            passwordResetHandler);

        _router.Register(
            PacketType.ResetPasswordRequest,
            passwordResetHandler);

        // Hoàng - Matchmaking
        var matchmakingHandler = new MatchmakingHandler(_matchmaking);

        _router.Register(
            PacketType.MatchmakingJoinRequest,
            matchmakingHandler);

        _router.Register(
            PacketType.MatchmakingCancelRequest,
            matchmakingHandler);

        // Friend Handler
        var friendHandler = new FriendHandler(_sessions);

        _router.Register(
            PacketType.GetFriendsRequest,
            friendHandler);

        // ========================================================
        // Đức - RoomHandler và Battleship Gameplay
        // ========================================================

        _roomHandler = new RoomHandler(_roomManager, _router);

        _router.Register(
            PacketType.GetRoomListRequest,
            _roomHandler);

        _router.Register(
            PacketType.CreateRoomRequest,
            _roomHandler);

        _router.Register(
            PacketType.JoinRoomRequest,
            _roomHandler);

        _router.Register(
            PacketType.LeaveRoom,
            _roomHandler);

        _router.Register(
            PacketType.KickPlayerRequest,
            _roomHandler);

        // Gameplay packets
        _router.Register(
            PacketType.PlaceShipsRequest,
            _roomHandler);

        _router.Register(
            PacketType.PlayerReady,
            _roomHandler);

        _router.Register(
            PacketType.FireShotRequest,
            _roomHandler);

        // ========================================================
        // Dũng - Match History
        // TODO: Đăng ký sự kiện lưu lịch sử khi hoàn thành DB.
        // ========================================================
    }

    // ============================================================
    // KHỞI ĐỘNG TCP SERVER
    // ============================================================

    public async Task StartAsync()
    {
        _listener.Start();

        Console.WriteLine(
            $"[TcpServer] Đang lắng nghe tại cổng " +
            $"{((IPEndPoint)_listener.LocalEndpoint).Port}...");

        // Worker ghép cặp chạy nền.
        _ = _matchmaking.RunAsync(_cts.Token);

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient client =
                    await _listener.AcceptTcpClientAsync(_cts.Token);

                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[TcpServer] Đã dừng lắng nghe.");
        }
    }

    // ============================================================
    // XỬ LÝ CLIENT KẾT NỐI
    // ============================================================

    private async Task HandleClientAsync(TcpClient client)
    {
        var session = new ClientSession(client);

        session.OnDisconnected += OnSessionDisconnected;
        session.OnPacketReceived += _router.RouteAsync;

        bool accepted = false;

        try
        {
            accepted = await session.PerformHandshakeAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[TcpServer] Lỗi lúc bắt tay " +
                $"({ex.GetType().Name}): {ex.Message}");
        }

        if (!accepted)
        {
            Console.WriteLine(
                $"[TcpServer] Handshake thất bại -> đóng session " +
                $"{session.SessionId}");

            session.Dispose();
            return;
        }

        _sessions.TryAdd(session.SessionId, session);

        Console.WriteLine(
            $"[TcpServer] Client {session.SessionId} đã kết nối. " +
            $"Online: {_sessions.Count}");

        await session.StartReceivingAsync(_cts.Token);
    }

    // ============================================================
    // XỬ LÝ KHI CLIENT MẤT KẾT NỐI
    // ============================================================

    private void OnSessionDisconnected(ClientSession session)
    {
        _sessions.TryRemove(session.SessionId, out _);

        OnlineUsers.Remove(session);

        _matchmaking.Cancel(session);

        Console.WriteLine(
            $"[TcpServer] Client {session.SessionId} đã ngắt. " +
            $"Online: {_sessions.Count}");

        // Dũng - Broadcast trạng thái Offline.
        if (session.IsLoggedIn &&
            !string.IsNullOrEmpty(session.Username))
        {
            var offlinePacket = new Packet
            {
                Type = PacketType.OnlineListUpdate,

                Payload = JsonSerializer.Serialize(
                    new
                    {
                        Username = session.Username,
                        Status = "Offline"
                    })
            };

            _ = _router.BroadcastAsync(offlinePacket);
        }

        // Đức - Xử lý khi người chơi mất kết nối.
        if (_roomHandler != null)
        {
            _ = HandleRoomDisconnectAsync(session);
        }
    }

    // Bọc xử lý disconnect để bắt exception từ tác vụ bất đồng bộ.
    private async Task HandleRoomDisconnectAsync(ClientSession session)
    {
        try
        {
            if (_roomHandler != null)
            {
                await _roomHandler.HandleSessionDisconnectedAsync(session);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[TcpServer] Lỗi xử lý Room disconnect: {ex.Message}");
        }
    }

    // ============================================================
    // DỪNG SERVER
    // ============================================================

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}
