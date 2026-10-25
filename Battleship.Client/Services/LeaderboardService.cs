using System.Net.Http;
using System.Text.Json;
using Battleship.Shared;

namespace Battleship.Client.Services;

// Lấy bảng xếp hạng từ web service (REST API) của Supabase.
// Gọi thẳng từ Client lên Supabase, không đi qua TCP Server.
public class LeaderboardService
{
    // Địa chỉ project và publishable key của Supabase.
    // Publishable key sinh ra để công khai trong ứng dụng client, dữ liệu được bảo vệ bằng RLS bên Supabase
    // (bảng gốc bị khóa, chỉ view "leaderboard" đọc được). KHÔNG BAO GIỜ để secret key / mật khẩu DB ở đây.
    // Muốn đổi sang project khác thì đặt biến môi trường SUPABASE_URL và SUPABASE_PUBLISHABLE_KEY.
    private const string DefaultUrl = "https://hqhggiixuqhhpuydparb.supabase.co";
    private const string DefaultKey = "sb_publishable__CEmQx8woYvxjvD0qUCeKg_4Gf8LMA3";

    private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    private readonly string _baseUrl;
    private readonly string _key;

    public LeaderboardService()
    {
        _baseUrl = (Environment.GetEnvironmentVariable("SUPABASE_URL") ?? DefaultUrl).TrimEnd('/');
        _key = Environment.GetEnvironmentVariable("SUPABASE_PUBLISHABLE_KEY") ?? DefaultKey;
    }

    // Lấy top N người chơi, điểm cao nhất lên đầu (bằng điểm thì ai nhiều trận thắng hơn đứng trước)
    public async Task<List<LeaderboardEntryDto>> GetTopAsync(int top = 10)
    {
        string url = _baseUrl + "/rest/v1/leaderboard?select=*&order=Score.desc,Wins.desc&limit=" + top;

        using (var req = new HttpRequestMessage(HttpMethod.Get, url))
        {
            req.Headers.Add("apikey", _key);   // chỉ cần header này là đủ

            using (var res = await _http.SendAsync(req))
            {
                string json = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Supabase trả về lỗi " + (int)res.StatusCode + ": " + json);
                }

                return JsonSerializer.Deserialize<List<LeaderboardEntryDto>>(json, JsonConfig.Options)
                       ?? new List<LeaderboardEntryDto>();
            }
        }
    }
}
