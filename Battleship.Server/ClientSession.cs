using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Server
{
    public class ClientSession : IDisposable
    {
        public string SessionId { get; private set; }
        public TcpClient Client { get; private set; }
        private readonly NetworkStream _stream;

        // Mục 4.6: Hàng đợi gửi riêng (Send Queue) cho từng Client + 1 Task ghi độc lập.
        // Một client mạng chậm sẽ chỉ tích tụ trong hàng đợi của chính nó, không làm nghẽn client khác.
        private readonly Channel<Packet> _sendChannel = Channel.CreateUnbounded<Packet>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        private readonly CancellationTokenSource _sessionCts = new CancellationTokenSource();
        private readonly Task _sendLoopTask;
        private int _cleanedUp = 0;

        // Tên đăng nhập & UserId, gán sau khi Dũng xử lý Login (phục vụ hiển thị Online/Offline).
        public string? Username { get; set; }
        public string UserId { get; set; } = ""; // string cho khớp User.Id
        public bool IsLoggedIn { get; set; } = false;

        // Delegate báo cho Server biết Client này đã ngắt kết nối
        public event Action<ClientSession>? OnDisconnected;

        // Mỗi khi parse xong 1 gói tin, bắn lên cho PacketRouter điều hướng (Nhiệm vụ 3.3).
        public event Func<ClientSession, Packet, Task>? OnPacketReceived;

        public ClientSession(TcpClient client)
        {
            SessionId = Guid.NewGuid().ToString("N");
            Client = client;
            _stream = client.GetStream();

            // Khởi chạy 1 Task duy nhất chuyên đọc từ _sendChannel và ghi xuống _stream
            _sendLoopTask = Task.Run(() => ProcessSendQueueAsync(_sessionCts.Token));
        }

        // Bắt tay phía Server trước khi vào vòng nhận gói nghiệp vụ.
        public Task<bool> PerformHandshakeAsync(CancellationToken ct = default)
            => HandshakeHelper.ServerHandshakeAsync(_stream, SessionId, ct);

        public async Task StartReceivingAsync(CancellationToken ct = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _sessionCts.Token);
            var token = linkedCts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Đọc trọn 1 gói theo Framing 4-byte (đủ PayloadLength mới trả về).
                    Packet? packet = await PacketFraming.ReadPacketAsync(_stream, token);
                    if (packet == null)
                    {
                        Console.WriteLine($"[Session {SessionId}] Client đóng kết nối bình thường.");
                        break;
                    }

                    // Chuyển gói đã parse cho Router xử lý
                    var handler = OnPacketReceived;
                    if (handler != null)
                        await handler(this, packet);
                }
            }
            catch (OperationCanceledException)
            {
                // Server hoặc Session chủ động dừng, không phải lỗi.
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

        // Bỏ gói tin vào hàng gửi ngay lập tức (không chờ I/O mạng)
        // Giữ nguyên kiểu trả về Task để tương thích 100% với code gọi bên ngoài (ví dụ Task.WhenAll)
        public Task SendAsync(Packet packet, CancellationToken ct = default)
        {
            if (_sessionCts.IsCancellationRequested || ct.IsCancellationRequested)
                return Task.CompletedTask;

            _sendChannel.Writer.TryWrite(packet);
            return Task.CompletedTask;
        }

        // Vòng lặp chạy nền duy nhất ghi xuống NetworkStream (đảm bảo tuần tự 100%, không xen kẽ byte)
        private async Task ProcessSendQueueAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var packet in _sendChannel.Reader.ReadAllAsync(ct))
                {
                    await PacketFraming.SendPacketAsync(_stream, packet, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Dừng hàng đợi bình thường khi đóng phiên
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Session {SessionId}] Lỗi ghi SendQueue: {ex.Message}");
                Cleanup();
            }
        }

        private void Cleanup()
        {
            // Đảm bảo Cleanup chỉ chạy đúng 1 lần dù cả luồng đọc và luồng ghi cùng gặp lỗi
            if (Interlocked.Exchange(ref _cleanedUp, 1) == 1)
                return;

            _sendChannel.Writer.TryComplete();
            _sessionCts.Cancel();

            try { _stream?.Close(); } catch { }
            try { Client?.Close(); } catch { }

            OnDisconnected?.Invoke(this); // Phát sự kiện để Server xóa khỏi danh sách Online
        }

        public void Dispose()
        {
            Cleanup();
            _sessionCts.Dispose();
        }
    }
}
