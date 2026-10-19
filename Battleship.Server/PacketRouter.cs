using Battleship.Client.Models;
using Battleship.Server.Helpers;
using Battleship.Server.Models;
using Battleship.Shared;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


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
                // ----- Tài khoản (phần của Dũng - 3.2) -----
                case PacketType.LoginRequest:
                case PacketType.RegisterRequest:
                    // TODO (Dũng): đọc LoginRequestDto, xác thực bằng PasswordHelper + AppDbContext,
                    // gán session.Username, rồi session.SendAsync(LoginResponse/RegisterResponse).
                    await ReplyNotImplemented(session, packet.Type);
                    break;

                // ----- Phòng đấu (phần của Đức - 3.4) -----
                case PacketType.CreateRoomRequest:
                case PacketType.JoinRoomRequest:
                case PacketType.LeaveRoom:
                    // TODO (Đức): dùng _roomManager.CreateRoom/JoinRoom/... rồi phát RoomUpdate.
                    await ReplyNotImplemented(session, packet.Type);
                    break;

                // ----- Gameplay (phần của Đức - 4.x) -----
                case PacketType.PlaceShipsRequest:
                case PacketType.PlayerReady:
                case PacketType.FireShotRequest:
                    // TODO (Đức): logic đặt tàu / sẵn sàng / bắn turn-based trên Server.
                    await ReplyNotImplemented(session, packet.Type);
                    break;

                // ----- Chat sảnh: Router xử lý trực tiếp bằng broadcast -----
                case PacketType.ChatMessage:
                    await BroadcastAsync(packet);
                    break;

                default:
                    Console.WriteLine($"[Router] Chưa định tuyến cho PacketType: {packet.Type} (từ {session.SessionId})");
                    break;
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
