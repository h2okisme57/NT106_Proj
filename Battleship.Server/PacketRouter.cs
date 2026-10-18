using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Bộ điều hướng gói tin theo cơ chế ĐĂNG KÝ HANDLER.
// - Mỗi PacketType được map tới 1 IPacketHandler trong _handlers.
// - Thành viên khác chỉ việc Register handler của mình, KHÔNG sửa code trong class này
//   => tránh conflict ở điểm nóng nhất.
public class PacketRouter
{
    private readonly ConcurrentDictionary<string, ClientSession> _sessions;
    private readonly Dictionary<PacketType, IPacketHandler> _handlers = new();

    public PacketRouter(ConcurrentDictionary<string, ClientSession> sessions)
    {
        _sessions = sessions;
    }

    // Đăng ký handler cho 1 loại gói. Gọi ở lúc khởi tạo server.
    public void Register(PacketType type, IPacketHandler handler)
    {
        _handlers[type] = handler;
    }

    public async Task RouteAsync(ClientSession session, Packet packet)
    {
        try
        {
            if (_handlers.TryGetValue(packet.Type, out var handler))
            {
                await handler.HandleAsync(session, packet);
            }
            else
            {
                Console.WriteLine($"[Router] Chưa có handler cho {packet.Type} (từ {session.SessionId})");
                await session.SendAsync(Packet.Create(PacketType.Error,
                    new ErrorDto { Message = $"Server chưa hỗ trợ {packet.Type}." }));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Router] Lỗi xử lý {packet.Type} từ {session.SessionId}: {ex.Message}");
        }
    }

    // Phát 1 gói cho mọi client đang online (handler gọi để chat sảnh / cập nhật online...).
    public async Task BroadcastAsync(Packet packet)
    {
        foreach (var s in _sessions.Values)
        {
            try
            {
                await s.SendAsync(packet);
            }
            catch
            {
                // Session lỗi sẽ tự được gỡ qua sự kiện OnDisconnected, bỏ qua ở đây.
            }
        }
    }

    // Phát cho mọi client TRỪ 1 session (vd: không gửi lại gói chat cho chính người gửi nếu muốn).
    public async Task BroadcastExceptAsync(Packet packet, ClientSession except)
    {
        foreach (var s in _sessions.Values)
        {
            if (s.SessionId == except.SessionId) continue;
            try { await s.SendAsync(packet); } catch { }
        }
    }
}
