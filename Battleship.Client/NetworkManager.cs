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

            Packet? packet = await PacketFraming.ReadPacketAsync(_stream, ct);

            if (packet != null && packet.Type == expectedType)
            {
                return packet;
            }

            return packet;
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