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

        /// <summary>
        /// Gửi 1 Packet xuống Server sử dụng PacketFraming
        /// </summary>
        public async Task SendAsync(Packet packet, CancellationToken ct = default)
        {
            if (!IsConnected || _stream == null) return;
            await PacketFraming.SendPacketAsync(_stream, packet, ct);
        }

        /// <summary>
        /// Đọc 1 Packet từ Server
        /// </summary>
        public async Task<Packet?> WaitForPacketAsync(PacketType expectedType, CancellationToken ct = default)
        {
            if (!IsConnected || _stream == null) return null;

            // Đọc trọn gói tin theo chuẩn framing 4-byte
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
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }
    }
}