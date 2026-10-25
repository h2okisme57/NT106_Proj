using Battleship.Server;
using Battleship.Server.Models;

const int Port = 8000; // Cổng server theo TinhNang-CongNghe (Load Balancer Port 8000)

// Tạo bảng nếu chưa có (SQLite tạo file mới, Supabase tạo bảng trên cloud).
// Trước đây không ai gọi nên DB mới toanh sẽ báo lỗi "no such table".
Console.WriteLine("[DB] Đang dùng: " + AppDbContext.ProviderName);
try
{
    AppDbContext.EnsureDatabaseReady();
    Console.WriteLine("[DB] Kết nối và kiểm tra bảng thành công.");
}
catch (Exception ex)
{
    Console.WriteLine("[DB] LỖI kết nối cơ sở dữ liệu: " + (ex.InnerException?.Message ?? ex.Message));
    Console.WriteLine("[DB] Gợi ý: nếu mất mạng, đặt BATTLESHIP_DB=sqlite rồi chạy lại.");
    return;
}

var server = new TcpServer(Port);

// Ctrl+C để dừng server êm (không giết tiến trình thô).
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[Program] Đang dừng server...");
    server.Stop();
};

Console.WriteLine("=== Battleship Server ===");
await server.StartAsync();
