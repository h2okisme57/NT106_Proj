using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Client
{
    public class NetworkManager
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance => _instance ??= new NetworkManager();

        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _receiveCts;

        // =======================================================
        // SỰ KIỆN: Phát tín hiệu mỗi khi nhận được Packet từ Server
        // =======================================================
        public event Action<Packet> OnPacketReceived;

        public bool IsConnected => _client != null && _client.Connected;

        // Lưu các gói vừa nhận để WaitForPacketAsync không bị lỡ khi server trả lời quá nhanh
        // (gói về trước khi kịp đăng ký lắng nghe). Mọi việc ĐỌC stream chỉ do ReceiveLoopAsync làm.
        private readonly object _recentLock = new object();
        private readonly List<(Packet Packet, DateTime At)> _recent = new List<(Packet Packet, DateTime At)>();
        private DateTime _lastSendAt = DateTime.MinValue;

        /// <summary>
        /// Mở kết nối TCP và thực hiện Bắt tay (Handshake) với Server
        /// </summary>
        public async Task<bool> ConnectAsync(string ip = "127.0.0.1", int port = 8000)
        {
            try
            {
                if (IsConnected) return true;

                _client = new TcpClient();
                await _client.ConnectAsync(ip, port);
                _stream = _client.GetStream();

                // Gọi HandshakeHelper chuẩn phía Client
                var ack = await HandshakeHelper.ClientHandshakeAsync(_stream, ProtocolInfo.Version.ToString(), "player");

                if (ack != null && ack.Accepted)
                {
                    Console.WriteLine($"[NetworkManager] Bắt tay thành công. SessionId: {ack.SessionId}");

                    // =======================================================
                    // KHỞI ĐỘNG LUỒNG LẮNG NGHE SAU KHI BẮT TAY THÀNH CÔNG
                    // =======================================================
                    _receiveCts = new CancellationTokenSource();
                    _ = ReceiveLoopAsync(_receiveCts.Token); // Fire-and-forget task

                    return true;
                }

                Console.WriteLine($"[NetworkManager] Bắt tay thất bại: {ack?.Message}");
                Disconnect();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NetworkManager] Lỗi kết nối: {ex.Message}");
                Disconnect();
                return false;
            }
        }

        // =======================================================
        // VÒNG LẶP CHẠY NGẦM ĐỂ ĐỌC GÓI TIN LIÊN TỤC
        // =======================================================
        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            try
            {
                while (IsConnected && !ct.IsCancellationRequested)
                {
                    // Đọc trọn gói tin theo chuẩn framing 4-byte
                    Packet? packet = await PacketFraming.ReadPacketAsync(_stream, ct);

                    if (packet != null)
                    {
                        // Cất vào bộ đệm trước, rồi mới báo sự kiện
                        lock (_recentLock)
                        {
                            _recent.Add((packet, DateTime.UtcNow));
                            if (_recent.Count > 50) _recent.RemoveAt(0);
                        }

                        // Kích hoạt sự kiện để Lobby (và các UI khác) xử lý
                        OnPacketReceived?.Invoke(packet);
                    }
                    else
                    {
                        // Nếu packet trả về null, tức là kết nối đã bị đóng từ phía Server
                        Console.WriteLine("[NetworkManager] Mất kết nối từ máy chủ.");
                        break;
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                Console.WriteLine($"[NetworkManager] Lỗi vòng lặp nhận dữ liệu: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Gửi 1 Packet xuống Server sử dụng PacketFraming
        /// </summary>
        public async Task SendAsync(Packet packet, CancellationToken ct = default)
        {
            if (!IsConnected || _stream == null) return;
            lock (_recentLock) { _lastSendAt = DateTime.UtcNow; }   // mốc: chỉ nhận phản hồi về SAU lần gửi này
            try
            {
                await PacketFraming.SendPacketAsync(_stream, packet, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NetworkManager] Lỗi khi gửi: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Đọc 1 Packet từ Server (Sử dụng song song với ReceiveLoopAsync có thể gây tranh chấp luồng. 
        /// Nên sử dụng OnPacketReceived để xử lý bất đồng bộ toàn diện hơn).
        /// </summary>
        public async Task<Packet?> WaitForPacketAsync(PacketType expectedType, CancellationToken ct = default)
        {
            if (!IsConnected || _stream == null) return null;

            // KHÔNG đọc stream ở đây nữa: vòng lặp nền ReceiveLoopAsync đã đọc stream rồi,
            // hai nơi cùng đọc một stream sẽ xé đôi khung gói tin (lỗi "Lỗi kết nối máy chủ!").
            // Thay vào đó chờ gói đúng loại mà vòng lặp nền nhận được.
            var tcs = new TaskCompletionSource<Packet?>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action<Packet> handler = p => { if (p.Type == expectedType) tcs.TrySetResult(p); };
            OnPacketReceived += handler;
            try
            {
                // Gói có thể đã về trước khi mình đăng ký lắng nghe: tìm trong bộ đệm (kể từ lần gửi gần nhất)
                lock (_recentLock)
                {
                    for (int i = _recent.Count - 1; i >= 0; i--)
                    {
                        if (_recent[i].Packet.Type == expectedType && _recent[i].At >= _lastSendAt)
                        {
                            tcs.TrySetResult(_recent[i].Packet);
                            _recent.RemoveAt(i);
                            break;
                        }
                    }
                }

                // Chờ tối đa 10 giây; quá hạn thì trả về null ("Không nhận được phản hồi")
                var timeout = Task.Delay(TimeSpan.FromSeconds(10), ct);
                var finished = await Task.WhenAny(tcs.Task, timeout);
                return finished == tcs.Task ? tcs.Task.Result : null;
            }
            finally
            {
                OnPacketReceived -= handler;
            }
        }

        /// <summary>
        /// Ngắt kết nối và dọn dẹp tài nguyên
        /// </summary>
        public void Disconnect()
        {
            _receiveCts?.Cancel();
            _receiveCts?.Dispose();
            _receiveCts = null;

            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }
    }
}