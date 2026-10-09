using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server;

// Bộ điều hướng: nhận mỗi Packet đã parse từ ClientSession rồi dispatch theo PacketType.
// Router CHỈ định tuyến + hạ tầng broadcast. Logic nghiệp vụ nằm ở:
//   - Dũng (3.2): xác thực Login/Register qua AppDbContext + PasswordHelper.
//   - Đức (3.4 / 4.x): tạo/vào phòng (RoomManager) và gameplay turn-based.
// Các điểm nối tới phần người khác được đánh dấu TODO để họ cắm code vào.
public class PacketRouter
{
    private readonly RoomManager _roomManager;
    private readonly ConcurrentDictionary<string, ClientSession> _sessions;

    public PacketRouter(RoomManager roomManager, ConcurrentDictionary<string, ClientSession> sessions)
    {
        _roomManager = roomManager;
        _sessions = sessions;
    }

    public async Task RouteAsync(ClientSession session, Packet packet)
    {
        try
        {
            switch (packet.Type)
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

    // Phát 1 gói cho mọi client đang online (dùng cho chat sảnh / cập nhật danh sách online).
    private async Task BroadcastAsync(Packet packet)
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

    private static Task ReplyNotImplemented(ClientSession session, PacketType type)
        => session.SendAsync(Packet.Create(PacketType.Error,
            new ChatMessageDto { Sender = "Server", Content = $"Chưa triển khai xử lý cho {type}." }));
}
