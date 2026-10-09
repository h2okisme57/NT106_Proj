using Battleship.Server;

const int Port = 8000; // Cổng server theo TinhNang-CongNghe (Load Balancer Port 8000)

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
