using Battleship.Server.Helpers;
using Battleship.Shared;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Server.Models;
using Battleship.Client.Models;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Server.Handlers
{
    public class AuthHandler : IPacketHandler
    {
        // Đăng nhập sai 5 lần trong 5 phút thì khóa 60 giây (tính riêng theo IP và theo tên tài khoản)
        private static readonly LoginRateLimiter _loginLimiter =
            new LoginRateLimiter(5, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(60));

        // Chống tạo tài khoản hàng loạt: mỗi IP đăng ký tối đa 5 lần trong 10 phút
        private static readonly LoginRateLimiter _registerLimiter =
            new LoginRateLimiter(5, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5));

        // Hash giả, dùng để vẫn chạy BCrypt khi tên tài khoản không tồn tại.
        // Nếu không thì "sai tên" trả lời nhanh hơn "sai mật khẩu", kẻ xấu nhìn thời gian là biết tên nào có thật.
        private static readonly string DummyHash = PasswordHelper.HashPassword("mat-khau-gia-de-chong-do-thoi-gian");

        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            if (packet.Type == PacketType.LoginRequest)
            {
                await HandleLoginAsync(session, packet);
            }
            else if (packet.Type == PacketType.RegisterRequest)
            {
                await HandleRegisterAsync(session, packet);
            }
        }

        private async Task HandleLoginAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<LoginRequestDto>(packet.Payload);
            if (req == null)
            {
                await SendLoginAsync(session, false, "", "Dữ liệu đăng nhập không hợp lệ.");
                return;
            }

            // Đã đăng nhập rồi thì không cho đăng nhập lại để đổi sang tài khoản khác trên cùng kết nối
            if (session.IsLoggedIn)
            {
                await SendLoginAsync(session, false, req.Username, "Bạn đã đăng nhập rồi.");
                return;
            }

            string name = (req.Username ?? "").Trim();
            string ipKey = "ip:" + GetIp(session);
            string userKey = "user:" + name.ToLowerInvariant();

            // Đang bị khóa vì sai nhiều lần thì từ chối luôn, không cần kiểm tra mật khẩu
            bool ipLocked = _loginLimiter.IsLocked(ipKey, out int wait1);
            bool userLocked = _loginLimiter.IsLocked(userKey, out int wait2);
            if (ipLocked || userLocked)
            {
                int wait = Math.Max(wait1, wait2);
                await SendLoginAsync(session, false, name, "Đăng nhập sai quá nhiều lần. Thử lại sau " + wait + " giây.");
                return;
            }

            // Tên hoặc mật khẩu sai định dạng thì cũng tính là 1 lần thử, và trả lời chung chung
            if (AuthValidator.CheckUsername(name) != null || string.IsNullOrEmpty(req.Password)
                || req.Password.Length > AuthValidator.MaxPasswordLength)
            {
                _loginLimiter.RegisterFailure(ipKey);
                await SendLoginAsync(session, false, name, "Sai tài khoản hoặc mật khẩu!");
                return;
            }

            User? user;
            using (var db = new AppDbContext())
            {
                // So sánh không phân biệt hoa thường: Admin và admin là cùng 1 tài khoản
                string lower = name.ToLowerInvariant();
                user = db.Users.FirstOrDefault(u => u.Username.ToLower() == lower);
            }

            // Luôn chạy BCrypt dù có user hay không (chống đo thời gian)
            bool passwordOk = PasswordHelper.VerifyPassword(req.Password, user?.Password ?? DummyHash);

            if (user == null || !passwordOk)
            {
                _loginLimiter.RegisterFailure(ipKey);
                _loginLimiter.RegisterFailure(userKey);
                // Không nói rõ là sai tên hay sai mật khẩu
                await SendLoginAsync(session, false, name, "Sai tài khoản hoặc mật khẩu!");
                return;
            }

            // Đúng mật khẩu rồi, nhưng tài khoản đang được dùng ở nơi khác
            if (!OnlineUsers.TryLogin(user.Username, session))
            {
                await SendLoginAsync(session, false, user.Username, "Tài khoản này đang được đăng nhập ở nơi khác.");
                return;
            }

            _loginLimiter.Reset(ipKey);
            _loginLimiter.Reset(userKey);

            session.UserId = user.Id;
            session.Username = user.Username;
            session.IsLoggedIn = true;

            await SendLoginAsync(session, true, user.Username, "Đăng nhập thành công!", user);
        }

        private async Task HandleRegisterAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<RegisterRequestDto>(packet.Payload);
            if (req == null)
            {
                await SendRegisterAsync(session, false, "Dữ liệu đăng ký không hợp lệ.");
                return;
            }

            string name = (req.Username ?? "").Trim();
            string ipKey = "ip:" + GetIp(session);

            if (_registerLimiter.IsLocked(ipKey, out int wait))
            {
                await SendRegisterAsync(session, false, "Bạn đăng ký quá nhiều lần. Thử lại sau " + wait + " giây.");
                return;
            }

            // Kiểm tra định dạng tên và mật khẩu
            string? error = AuthValidator.CheckUsername(name) ?? AuthValidator.CheckPassword(req.Password, name);
            if (error != null)
            {
                await SendRegisterAsync(session, false, error);
                return;
            }

            // Mỗi lần gọi đăng ký (kể cả thành công) đều tính vào giới hạn của IP
            _registerLimiter.RegisterFailure(ipKey);

            using (var db = new AppDbContext())
            {
                string lower = name.ToLowerInvariant();

                // Không phân biệt hoa thường để không ai đăng ký "Admin" giả mạo "admin"
                if (db.Users.Any(u => u.Username.ToLower() == lower))
                {
                    await SendRegisterAsync(session, false, "Tên tài khoản đã tồn tại!");
                    return;
                }

                var newUser = new User { Username = name, Password = PasswordHelper.HashPassword(req.Password) };
                db.Users.Add(newUser);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    // 2 người đăng ký cùng 1 tên trong cùng 1 tích tắc: DB có ràng buộc UNIQUE nên 1 người bị lỗi ở đây
                    await SendRegisterAsync(session, false, "Tên tài khoản đã tồn tại!");
                    return;
                }
            }

            await SendRegisterAsync(session, true, "Đăng ký thành công!");
        }

        // Lấy địa chỉ IP của client (bỏ số cổng để cùng 1 máy mở nhiều kết nối vẫn tính chung)
        private static string GetIp(ClientSession session)
        {
            try
            {
                var ep = session.Client.Client.RemoteEndPoint as IPEndPoint;
                return ep?.Address.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        // user chỉ truyền khi đăng nhập thành công: điền thêm thông tin cho trang Profile của Client
        private static Task SendLoginAsync(ClientSession session, bool ok, string username, string message, User? user = null)
        {
            var res = new LoginResponseDto { Success = ok, Username = username, Message = message };
            if (user != null)
            {
                res.UserId = user.Id;
                res.Wins = user.Wins;
                res.Losses = user.Losses;
                res.Score = user.Score;
            }
            return session.SendAsync(new Packet { Type = PacketType.LoginResponse, Payload = JsonSerializer.Serialize(res) });
        }

        private static Task SendRegisterAsync(ClientSession session, bool ok, string message)
        {
            var res = new RegisterResponseDto { Success = ok, Message = message };
            return session.SendAsync(new Packet { Type = PacketType.RegisterResponse, Payload = JsonSerializer.Serialize(res) });
        }
    }
}
