using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Battleship.Shared;

namespace Battleship.Client;

// Lớp networking phía Client: kết nối tới Server, bắt tay (handshake),
// chạy vòng lặp nhận gói ở luồng nền và cho phép gửi gói bất đồng bộ.
//
// ⚠️ LƯU Ý CHO UI (Dũng): sự kiện PacketReceived / Disconnected được bắn từ LUỒNG NỀN.
//    Khi cập nhật control WinForms trong các handler này, BẮT BUỘC dùng Invoke/BeginInvoke:
//        NetworkClient.Instance.PacketReceived += p =>
//            this.BeginInvoke(() => XuLyGoiTrenUiThread(p));
public class NetworkClient
{
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
    private CancellationTokenSource? _cts;
    private bool _disconnectedRaised;

    public bool IsConnected => _tcp?.Connected == true;
    public string? SessionId { get; private set; }

    // Mỗi khi nhận trọn 1 gói -> bắn ra đây (trên luồng nền, xem cảnh báo ở trên).
    public event Action<Packet>? PacketReceived;
    // Khi mất kết nối / server đóng.
    public event Action? Disconnected;

    // Singleton dùng chung 1 kết nối giữa các Form (Login -> Lobby -> Bàn cờ).
    public static NetworkClient Instance { get; } = new NetworkClient();

    // Kết nối + bắt tay. Trả về Ack (null nếu không nối được / server đóng giữa chừng).
    // ack.Accepted == false nghĩa là lệch phiên bản giao thức.
    public async Task<HandshakeAckDto?> ConnectAsync(
        string host, int port, string clientVersion = "1.0", string role = "player")
    {
        _tcp = new TcpClient();
        await _tcp.ConnectAsync(host, port);
        _stream = _tcp.GetStream();
        _cts = new CancellationTokenSource();
        _disconnectedRaised = false;

        HandshakeAckDto? ack = await HandshakeHelper.ClientHandshakeAsync(_stream, clientVersion, role, _cts.Token);
        if (ack == null || !ack.Accepted)
        {
            Disconnect();
            return ack;
        }

        SessionId = ack.SessionId;
        _ = ReceiveLoopAsync(_cts.Token); // fire-and-forget, chạy nền
        return ack;
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _stream != null)
            {
                Packet? packet = await PacketFraming.ReadPacketAsync(_stream, ct);
                if (packet == null) break; // server đóng kết nối
                PacketReceived?.Invoke(packet);
            }
        }
        catch (OperationCanceledException)
        {
            // chủ động ngắt, không phải lỗi
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NetworkClient] Lỗi nhận gói: {ex.Message}");
        }
        finally
        {
            RaiseDisconnected();
        }
    }

    // Gửi 1 gói (an toàn khi nhiều chỗ cùng gửi).
    public async Task SendAsync(Packet packet)
    {
        if (_stream == null)
            throw new InvalidOperationException("Chưa kết nối tới server.");

        await _sendLock.WaitAsync();
        try
        {
            await PacketFraming.SendPacketAsync(_stream, packet);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    // Tiện ích: đóng gói data theo PacketType rồi gửi luôn.
    public Task SendAsync(PacketType type, object data) => SendAsync(Packet.Create(type, data));

    public void Disconnect()
    {
        try { _cts?.Cancel(); } catch { /* ignore */ }
        try { _stream?.Close(); } catch { /* ignore */ }
        try { _tcp?.Close(); } catch { /* ignore */ }
        RaiseDisconnected();
    }

    private void RaiseDisconnected()
    {
        if (_disconnectedRaised) return;
        _disconnectedRaised = true;
        Disconnected?.Invoke();
    }
}
