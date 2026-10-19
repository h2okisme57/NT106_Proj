using Battleship.Server.Helpers;
using Battleship.Shared;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Server.Models;
using Battleship.Client.Models;

namespace Battleship.Server.Handlers
{
    public class AuthHandler : IPacketHandler
    {
        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            if (packet.Type == PacketType.LoginRequest)
            {
                var req = JsonSerializer.Deserialize<LoginRequestDto>(packet.Payload);
                if (req == null) return;

                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == req.Username);

                    if (user != null && PasswordHelper.VerifyPassword(req.Password, user.Password))
                    {
                        session.UserId = user.Id.ToString();
                        session.Username = user.Username;
                        session.IsLoggedIn = true;

                        var res = new LoginResponseDto { Success = true, Username = user.Username, Message = "Đăng nhập thành công!" };
                        await session.SendAsync(new Packet { Type = PacketType.LoginResponse, Payload = JsonSerializer.Serialize(res) });
                    }
                    else
                    {
                        // ĐÃ SỬA LỖI CRASH: Dùng req.Username vì user có thể bị null
                        var res = new LoginResponseDto { Success = false, Username = req.Username, Message = "Sai tài khoản hoặc mật khẩu!" };
                        await session.SendAsync(new Packet { Type = PacketType.LoginResponse, Payload = JsonSerializer.Serialize(res) });
                    }
                }
            }
            else if (packet.Type == PacketType.RegisterRequest)
            {
                var req = JsonSerializer.Deserialize<RegisterRequestDto>(packet.Payload);
                if (req == null) return;

                using (var db = new AppDbContext())
                {
                    if (db.Users.Any(u => u.Username == req.Username))
                    {
                        var res = new RegisterResponseDto { Success = false, Message = "Tên tài khoản đã tồn tại!" };
                        await session.SendAsync(new Packet { Type = PacketType.RegisterResponse, Payload = JsonSerializer.Serialize(res) });
                    }
                    else
                    {
                        var newUser = new User { Username = req.Username, Password = PasswordHelper.HashPassword(req.Password) };
                        db.Users.Add(newUser);
                        db.SaveChanges();

                        var res = new RegisterResponseDto { Success = true, Message = "Đăng ký thành công!" };
                        await session.SendAsync(new Packet { Type = PacketType.RegisterResponse, Payload = JsonSerializer.Serialize(res) });
                    }
                }
            }
        }
    }
}