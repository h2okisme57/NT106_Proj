using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

public class ClientSession : IDisposable
{
    public string SessionId { get; private set; }
    public TcpClient Client { get; private set; }
    private NetworkStream _stream;

    // Khóa gửi: chặn 2 Task cùng ghi vào 1 stream làm xen kẽ byte của 2 gói.
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

    // Tên đăng nhập, gán sau khi Dũng xử lý Login (phục vụ hiển thị Online/Offline).
    public string? Username { get; set; }

    // Delegate báo cho Server biết Client này đã ngắt kết nối
    public event Action<ClientSession>? OnDisconnected;

    // Mỗi khi parse xong 1 gói tin, bắn lên cho PacketRouter điều hướng (Nhiệm vụ 3.3).
    public event Func<ClientSession, Packet, Task>? OnPacketReceived;

    public ClientSession(TcpClient client)
    {
        SessionId = Guid.NewGuid().ToString("N");
        Client = client;
        _stream = client.GetStream();
    }

    // Bắt tay phía Server trước khi vào vòng nhận gói nghiệp vụ.
    public Task<bool> PerformHandshakeAsync(CancellationToken ct = default)
        => HandshakeHelper.ServerHandshakeAsync(_stream, SessionId, ct);

    public async Task StartReceivingAsync(CancellationToken ct = default)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Đọc trọn 1 gói theo Framing 4-byte (đủ PayloadLength mới trả về).
                Packet? packet = await PacketFraming.ReadPacketAsync(_stream, ct);

                if (packet == null)
                {
                    Console.WriteLine($"[Session {SessionId}] Client đóng kết nối bình thường.");
                    break;
                }

                // Chuyển gói đã parse cho Router xử lý (thay cho TODO cũ của Thành viên A).
                var handler = OnPacketReceived;
                if (handler != null)
                    await handler(this, packet);
            }
        }
        catch (OperationCanceledException)
        {
            // Server chủ động dừng, không phải lỗi.
        }
        catch (IOException ex) // Xử lý triệt để như Roadmap yêu cầu khi đứt cáp/lag
        {
            Console.WriteLine($"[Session {SessionId}] Đứt kết nối mạng (IOException): {ex.Message}");
        }
        catch (SocketException ex) // Client crash hoặc End Task đột ngột
        {
            Console.WriteLine($"[Session {SessionId}] Lỗi Socket (SocketException): {ex.Message}");
        }
        catch (InvalidDataException ex) // Gói tin sai định dạng framing
        {
            Console.WriteLine($"[Session {SessionId}] Gói tin không hợp lệ: {ex.Message}");
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

    // Gửi 1 gói tin xuống client này (an toàn khi nhiều Task cùng gọi).
    public async Task SendAsync(Packet packet, CancellationToken ct = default)
    {
        await _sendLock.WaitAsync(ct);
        try
        {
            await PacketFraming.SendPacketAsync(_stream, packet, ct);
        }
        finally
        {
            _sendLock.Release();
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
