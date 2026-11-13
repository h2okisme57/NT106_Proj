using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Battleship.Server;

// Gửi tin nhắn ra kênh Discord qua webhook (HTTP POST).
// Cách dùng: đặt biến môi trường DISCORD_WEBHOOK_URL = đường dẫn webhook của kênh.
// Đường dẫn này là BÍ MẬT (ai có nó là gửi tin được vào kênh), nên không ghi vào code,
// không commit lên Git, và cũng không in ra log.
public static class DiscordNotifier
{
    // dùng chung 1 HttpClient cho cả chương trình, đặt thời hạn 5 giây
    // để Discord chậm cũng không làm treo Server
    private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

    // Discord giới hạn nội dung tối đa 2000 ký tự
    private const int MaxLength = 2000;

    // Gửi 1 tin lên kênh (lấy đường dẫn từ biến môi trường).
    // Trả về true nếu gửi được. KHÔNG bao giờ ném lỗi ra ngoài: lỗi chỉ ghi log,
    // vì chuyện gửi thông báo thất bại không được làm hỏng ván đấu.
    public static Task<bool> SendAsync(string message)
    {
        string? url = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");
        if (string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("[Discord] Chưa đặt DISCORD_WEBHOOK_URL nên bỏ qua việc gửi thông báo.");
            return Task.FromResult(false);
        }
        return SendAsync(url, message);
    }

    // Gửi tới một đường dẫn cụ thể (dùng khi kiểm thử bằng webhook giả)
    public static async Task<bool> SendAsync(string webhookUrl, string message)
    {
        try
        {
            if (message.Length > MaxLength)
            {
                message = message.Substring(0, MaxLength);
            }

            // Discord nhận JSON dạng {"content": "nội dung tin nhắn"}
            string json = JsonSerializer.Serialize(new { content = message });
            using var body = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage resp = await _http.PostAsync(webhookUrl, body);

            // thành công thì Discord trả 204 (No Content)
            if (resp.IsSuccessStatusCode)
            {
                return true;
            }

            Console.WriteLine("[Discord] Gửi không thành công, mã trạng thái: " + (int)resp.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            // hết thời hạn, mất mạng, đường dẫn sai... đều chỉ ghi log
            Console.WriteLine("[Discord] Lỗi khi gửi: " + ex.Message);
            return false;
        }
    }

    // Tin nhắn thông báo kết quả một trận (gọi khi trận kết thúc)
    public static Task<bool> NotifyMatchResultAsync(string winner, string loser, int turns)
    {
        string msg = "Trận kết thúc: " + winner + " thắng " + loser + " sau " + turns + " lượt.";
        return SendAsync(msg);
    }
}
