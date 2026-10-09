using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Lõi mạng Server: lắng nghe TcpListener bất đồng bộ, mỗi client 1 Task riêng,
// quản lý danh sách session online bằng ConcurrentDictionary (thread-safe).
public class TcpServer
{
    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();
    private readonly RoomManager _roomManager = new();   // phần của Đức (đã có sẵn)
    private readonly PacketRouter _router;
    private readonly CancellationTokenSource _cts = new();

    public TcpServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _router = new PacketRouter(_roomManager, _sessions);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"[TcpServer] Đang lắng nghe tại cổng {((IPEndPoint)_listener.LocalEndpoint).Port}...");

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(_cts.Token);
                // Fire-and-forget: không await để vòng accept tiếp tục nhận client khác.
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[TcpServer] Đã dừng lắng nghe.");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        var session = new ClientSession(client);
        session.OnDisconnected += OnSessionDisconnected;
        session.OnPacketReceived += _router.RouteAsync;

        // Bắt tay kiểm tra phiên bản giao thức trước khi nhận gói nghiệp vụ.
        bool accepted = await session.PerformHandshakeAsync(_cts.Token);
        if (!accepted)
        {
            Console.WriteLine($"[TcpServer] Handshake thất bại -> đóng session {session.SessionId}");
            session.Dispose();
            return;
        }

        _sessions.TryAdd(session.SessionId, session);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã kết nối. Online: {_sessions.Count}");

        await session.StartReceivingAsync(_cts.Token);
    }

    private void OnSessionDisconnected(ClientSession session)
    {
        _sessions.TryRemove(session.SessionId, out _);
        Console.WriteLine($"[TcpServer] Client {session.SessionId} đã ngắt. Online: {_sessions.Count}");
        // TODO (Dũng - 3.2): broadcast trạng thái Offline cho các client còn lại.
        // TODO (Đức - 3.4): nếu session đang trong phòng thì xử lý rời/huỷ phòng.
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }
}
