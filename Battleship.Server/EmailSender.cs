using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Battleship.Server;

// Gửi email bằng SMTP (dùng cho tính năng quên mật khẩu).
// Cấu hình bằng biến môi trường (KHÔNG ghi vào code, không commit lên Git):
//   SMTP_USER      địa chỉ email dùng để gửi, ví dụ nhom@gmail.com
//   SMTP_PASSWORD  với Gmail phải là "Mật khẩu ứng dụng" (App Password), không phải mật khẩu đăng nhập Gmail
//   SMTP_HOST      mặc định smtp.gmail.com
//   SMTP_PORT      mặc định 587 (STARTTLS)
//   SMTP_FROM      địa chỉ hiển thị người gửi, mặc định bằng SMTP_USER
public static class EmailSender
{
    // Chỉ dùng khi kiểm thử: nếu được gán thì gửi qua hàm này thay vì SMTP thật
    public static Func<string, string, string, Task<bool>>? Override;

    // Đã cấu hình SMTP chưa
    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_USER")) &&
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMTP_PASSWORD"));

    // Gửi 1 email. Trả về true nếu gửi được. Không ném lỗi ra ngoài, lỗi chỉ ghi log
    // (và không bao giờ in mật khẩu SMTP ra log).
    public static async Task<bool> SendAsync(string to, string subject, string body)
    {
        if (Override != null)
        {
            return await Override(to, subject, body);
        }

        string? user = Environment.GetEnvironmentVariable("SMTP_USER");
        string? pass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine("[Email] Chưa đặt SMTP_USER / SMTP_PASSWORD nên không gửi được email.");
            return false;
        }

        string host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
        int port = 587;
        int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out port);
        if (port <= 0) port = 587;
        string from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? user;

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,                      // mã hóa kết nối tới máy chủ mail (STARTTLS)
                Credentials = new NetworkCredential(user, pass),
                Timeout = 10000                        // 10 giây, tránh treo khi mạng chặn cổng
            };
            using var mail = new MailMessage(from, to, subject, body);
            await client.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Email] Gửi thất bại: " + ex.Message);
            return false;
        }
    }
}
