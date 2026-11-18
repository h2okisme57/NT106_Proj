using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Battleship.Client.Models;   // lớp User nằm ở namespace này (tên cũ của nhóm)
using Battleship.Server.Helpers;
using Battleship.Server.Models;
using Battleship.Shared;

namespace Battleship.Server.Handlers
{
    // Quên mật khẩu qua email, gồm 2 bước:
    //   1. Client gửi ForgotPasswordRequest (tên tài khoản) -> Server gửi mã 6 số tới email của tài khoản đó.
    //   2. Client gửi ResetPasswordRequest (tên, mã, mật khẩu mới) -> Server kiểm tra mã rồi đổi mật khẩu.
    public class PasswordResetHandler : IPacketHandler
    {
        private const int CodeMinutes = 10;        // mã có hiệu lực 10 phút
        private const int MaxWrongCodes = 5;       // nhập sai mã 5 lần thì mã bị hủy
        private static readonly TimeSpan ResendWait = TimeSpan.FromSeconds(60);   // xin mã lại phải chờ 60 giây

        // Mỗi IP chỉ được xin mã tối đa 5 lần trong 10 phút (chống dùng Server để gửi thư rác)
        private static readonly LoginRateLimiter _requestLimiter =
            new LoginRateLimiter(5, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

        private class ResetEntry
        {
            public string Code = "";
            public DateTime ExpiresAt;
            public DateTime SentAt;
            public int WrongCount;
        }

        // tên tài khoản (chữ thường) -> mã đang chờ. Lưu trong bộ nhớ, tắt Server là mất, đúng ý muốn
        private static readonly ConcurrentDictionary<string, ResetEntry> _pending =
            new ConcurrentDictionary<string, ResetEntry>();

        // Thông báo chung dù tài khoản có tồn tại hay không, để người lạ không dò được tên nào có thật
        private const string GenericSentMessage =
            "Nếu tài khoản tồn tại và đã có email, mã xác nhận đã được gửi. Mã có hiệu lực 10 phút.";

        public async Task HandleAsync(ClientSession session, Packet packet)
        {
            if (packet.Type == PacketType.ForgotPasswordRequest)
            {
                await HandleForgotAsync(session, packet);
            }
            else if (packet.Type == PacketType.ResetPasswordRequest)
            {
                await HandleResetAsync(session, packet);
            }
        }

        // ---------- Bước 1: xin mã ----------
        private async Task HandleForgotAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<ForgotPasswordRequestDto>(packet.Payload);
            string name = (req?.Username ?? "").Trim();

            if (AuthValidator.CheckUsername(name) != null)
            {
                await SendForgotAsync(session, false, "Tên đăng nhập không hợp lệ.");
                return;
            }

            string ipKey = "forgot:" + GetIp(session);
            if (_requestLimiter.IsLocked(ipKey, out int wait))
            {
                await SendForgotAsync(session, false, "Bạn xin mã quá nhiều lần. Thử lại sau " + wait + " giây.");
                return;
            }
            _requestLimiter.RegisterFailure(ipKey);   // mỗi lần xin mã đều tính vào giới hạn

            string lower = name.ToLowerInvariant();

            // Vừa xin mã chưa lâu thì bắt chờ (chống bấm liên tục làm tràn hộp thư người ta)
            if (_pending.TryGetValue(lower, out var old) && DateTime.UtcNow - old.SentAt < ResendWait)
            {
                int left = (int)Math.Ceiling((ResendWait - (DateTime.UtcNow - old.SentAt)).TotalSeconds);
                await SendForgotAsync(session, false, "Vừa gửi mã rồi. Chờ " + left + " giây để xin lại.");
                return;
            }

            User? user;
            using (var db = new AppDbContext())
            {
                user = db.Users.FirstOrDefault(u => u.Username.ToLower() == lower);
            }

            // Không có tài khoản hoặc tài khoản cũ chưa có email: vẫn trả lời giống hệt như đã gửi
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                Console.WriteLine("[Reset] Không gửi mã cho '" + name + "' (không có tài khoản hoặc chưa có email).");
                await SendForgotAsync(session, true, GenericSentMessage);
                return;
            }

            string code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            string body = "Xin chào " + user.Username + ",\n\n"
                        + "Mã xác nhận đặt lại mật khẩu Battleship của bạn là: " + code + "\n"
                        + "Mã có hiệu lực trong " + CodeMinutes + " phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.\n";

            bool sent;
            if (EmailSender.IsConfigured || EmailSender.Override != null)
            {
                sent = await EmailSender.SendAsync(user.Email!, "Battleship: mã đặt lại mật khẩu", body);
            }
            else if (Environment.GetEnvironmentVariable("PASSWORD_RESET_DEBUG") == "1")
            {
                // Chỉ để chạy thử trên máy mình khi chưa có SMTP: in mã ra cửa sổ Server
                Console.WriteLine("[Reset][DEBUG] Mã của '" + user.Username + "' là " + code);
                sent = true;
            }
            else
            {
                Console.WriteLine("[Reset] Chưa cấu hình SMTP (SMTP_USER, SMTP_PASSWORD) nên không gửi được mã.");
                sent = false;
            }

            if (!sent)
            {
                await SendForgotAsync(session, false, "Máy chủ chưa gửi được email. Vui lòng thử lại sau.");
                return;
            }

            _pending[lower] = new ResetEntry
            {
                Code = code,
                SentAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(CodeMinutes)
            };
            Console.WriteLine("[Reset] Đã gửi mã tới email của '" + user.Username + "'.");
            await SendForgotAsync(session, true, GenericSentMessage);
        }

        // ---------- Bước 2: nhập mã và đặt mật khẩu mới ----------
        private async Task HandleResetAsync(ClientSession session, Packet packet)
        {
            var req = JsonSerializer.Deserialize<ResetPasswordRequestDto>(packet.Payload);
            if (req == null)
            {
                await SendResetAsync(session, false, "Dữ liệu không hợp lệ.");
                return;
            }

            string name = (req.Username ?? "").Trim();
            string lower = name.ToLowerInvariant();
            string code = (req.Code ?? "").Trim();

            string? error = AuthValidator.CheckUsername(name) ?? AuthValidator.CheckPassword(req.NewPassword, name);
            if (error != null)
            {
                await SendResetAsync(session, false, error);
                return;
            }

            // Chung một thông báo cho mọi lý do "mã không dùng được", để không lộ thông tin
            const string badCode = "Mã không đúng hoặc đã hết hạn.";

            if (!_pending.TryGetValue(lower, out var entry) || DateTime.UtcNow > entry.ExpiresAt)
            {
                _pending.TryRemove(lower, out _);
                await SendResetAsync(session, false, badCode);
                return;
            }

            // So sánh mã với thời gian cố định (không để kẻ xấu đo thời gian mà đoán từng chữ số)
            bool dung = code.Length == entry.Code.Length &&
                        CryptographicOperations.FixedTimeEquals(
                            System.Text.Encoding.UTF8.GetBytes(code),
                            System.Text.Encoding.UTF8.GetBytes(entry.Code));

            if (!dung)
            {
                entry.WrongCount++;
                if (entry.WrongCount >= MaxWrongCodes)
                {
                    _pending.TryRemove(lower, out _);   // sai quá nhiều thì hủy mã, phải xin mã mới
                    await SendResetAsync(session, false, "Nhập sai quá nhiều lần. Hãy xin mã mới.");
                }
                else
                {
                    await SendResetAsync(session, false, badCode);
                }
                return;
            }

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username.ToLower() == lower);
                if (user == null)
                {
                    await SendResetAsync(session, false, badCode);
                    return;
                }

                user.Password = PasswordHelper.HashPassword(req.NewPassword);
                db.SaveChanges();
            }

            _pending.TryRemove(lower, out _);   // mã chỉ dùng được một lần
            Console.WriteLine("[Reset] Đã đặt lại mật khẩu cho '" + name + "'.");
            await SendResetAsync(session, true, "Đặt lại mật khẩu thành công. Hãy đăng nhập bằng mật khẩu mới.");
        }

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

        private static Task SendForgotAsync(ClientSession session, bool ok, string message)
        {
            var res = new ForgotPasswordResponseDto { Success = ok, Message = message };
            return session.SendAsync(new Packet { Type = PacketType.ForgotPasswordResponse, Payload = JsonSerializer.Serialize(res) });
        }

        private static Task SendResetAsync(ClientSession session, bool ok, string message)
        {
            var res = new ResetPasswordResponseDto { Success = ok, Message = message };
            return session.SendAsync(new Packet { Type = PacketType.ResetPasswordResponse, Payload = JsonSerializer.Serialize(res) });
        }
    }
}
