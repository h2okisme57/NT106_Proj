using Battleship.Client.Models;
using Battleship.Server.Helpers;
using Battleship.Server.Models;
using Battleship.Shared;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;


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
                    {
                        var req = JsonSerializer.Deserialize<LoginRequestDto>(packet.Payload);

                        using (var db = new AppDbContext())
                        {
                            var user = db.Users.FirstOrDefault(u => u.Username == req.Username);

                            if (user != null && PasswordHelper.VerifyPassword(req.Password, user.Password))
                            {
                                session.UserId = user.Id;
                                session.Username = user.Username;
                                session.IsLoggedIn = true;

                                var res = new AuthResponse { IsSuccess = true, Username = user.Username };
                                // Đóng gói thành new Packet trước khi gửi
                                await session.SendAsync(new Packet
                                {
                                    Type = PacketType.LoginResponse,
                                    Payload = JsonSerializer.Serialize(res)
                                });
                            }
                            else
                            {
                                var res = new AuthResponse { IsSuccess = false, Username = user.Username };
                                await session.SendAsync(new Packet
                                {
                                    Type = PacketType.LoginResponse,
                                    Payload = JsonSerializer.Serialize(res)
                                });
                            }
                        }
                        break;
                    }

                case PacketType.RegisterRequest:
                    {
                        var req = JsonSerializer.Deserialize<LoginRequestDto>(packet.Payload);

                        using (var db = new AppDbContext())
                        {
                            if (db.Users.Any(u => u.Username == req.Username))
                            {
                                var res = new AuthResponse { IsSuccess = false };
                                
                                await session.SendAsync(new Packet
                                {
                                    Type = PacketType.RegisterResponse,
                                    Payload = JsonSerializer.Serialize(res)
                                });
                            }
                            else
                            {
                                string hash = PasswordHelper.HashPassword(req.Password);
                                var newUser = new User { Username = req.Username, Password = hash };

                                db.Users.Add(newUser);
                                await db.SaveChangesAsync();

                                var res = new AuthResponse { IsSuccess = true };
                                
                                await session.SendAsync(new Packet
                                {
                                    Type = PacketType.RegisterResponse,
                                    Payload = JsonSerializer.Serialize(res)
                                });
                            }
                        }
                        break;
                    }

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
