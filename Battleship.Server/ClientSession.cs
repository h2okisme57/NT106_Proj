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

        // Mục 4.6: Hàng đợi gửi có giới hạn (BoundedChannel 1000 gói) + 1 Task ghi độc lập.
        // Client mạng chậm tích tụ quá 1000 gói sẽ bị ngắt kết nối ngay để bảo vệ RAM Server.
        private const int MaxSendQueueCapacity = 1000;

        private readonly Channel<Packet> _sendChannel = Channel.CreateBounded<Packet>(
            new BoundedChannelOptions(MaxSendQueueCapacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            });

        private readonly CancellationTokenSource _sessionCts = new CancellationTokenSource();
        private readonly Task _sendLoopTask;
        private int _cleanedUp = 0;

        // Theo dõi mốc thời gian nhận Pong gần nhất (thread-safe bằng Interlocked)
        private long _lastPongTicks = DateTime.UtcNow.Ticks;
        private string? _username;

        // Tên đăng nhập & UserId, gán sau khi Dũng xử lý Login (phục vụ hiển thị Online/Offline).
        public string? Username
        {
            get => _username;
            set
            {
                _username = value;
                if (!string.IsNullOrEmpty(value))
                {
                    MarkPongReceived();
                }
            }
        }

        public string UserId { get; set; } = ""; // string cho khớp User.Id
        public bool IsLoggedIn { get; set; } = false;

        public void MarkPongReceived()
            => Interlocked.Exchange(ref _lastPongTicks, DateTime.UtcNow.Ticks);

        public DateTime LastPongUtc
            => new DateTime(Interlocked.Read(ref _lastPongTicks), DateTimeKind.Utc);

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

        // Bỏ gói tin vào hàng gửi có giới hạn ngay lập tức (không chờ I/O mạng).
        // Nếu hàng đợi chạm ngưỡng 1000 gói (client quá chậm), ngắt riêng phiên đó.
        public Task SendAsync(Packet packet, CancellationToken ct = default)
        {
            if (_sessionCts.IsCancellationRequested || ct.IsCancellationRequested)
                return Task.CompletedTask;

            if (!_sendChannel.Writer.TryWrite(packet))
            {
                ForceDisconnect($"Hàng đợi gửi đầy ({MaxSendQueueCapacity} gói) - client chậm");
            }

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

        // Chủ động ngắt kết nối (Heartbeat timeout hoặc tràn hàng đợi gửi)
        public void ForceDisconnect(string reason)
        {
            Console.WriteLine($"[Session {SessionId}] Ngắt kết nối chủ động ({reason}).");
            Cleanup();
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
