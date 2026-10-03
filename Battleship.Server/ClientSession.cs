using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class ClientSession : IDisposable
{
    public string SessionId { get; private set; }
    public TcpClient Client { get; private set; }
    private NetworkStream _stream;

    // Delegate báo cho Server biết Client này đã ngắt kết nối
    public event Action<ClientSession> OnDisconnected;

    public ClientSession(TcpClient client)
    {
        SessionId = Guid.NewGuid().ToString("N");
        Client = client;
        _stream = client.GetStream();
    }

    public async Task StartReceivingAsync()
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                // Lắng nghe bất đồng bộ, không block luồng chính
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine($"[Session {SessionId}] Client đóng kết nối bình thường.");
                    break;
                }

                // TODO: Chuyển mảng byte này cho Module Framing (Nhiệm vụ 3.3 của Thành viên C) để parse JSON
            }
        }
        catch (IOException ex) // Xử lý triệt để như Roadmap yêu cầu khi đứt cáp/lag
        {
            Console.WriteLine($"[Session {SessionId}] Đứt kết nối mạng (IOException): {ex.Message}");
        }
        catch (SocketException ex) // Client crash hoặc End Task đột ngột
        {
            Console.WriteLine($"[Session {SessionId}] Lỗi Socket (SocketException): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Session {SessionId}] Lỗi không xác định: {ex.Message}");
        }
        finally
        {
            // Tự kích hoạt Cleanup Session
            Cleanup();
        }
    }

    private void Cleanup()
    {
        _stream?.Close();
        Client?.Close();
        OnDisconnected?.Invoke(this); // Phát sự kiện để Server xóa khỏi danh sách Online
    }

    public void Dispose()
    {
        Cleanup();
    }
}