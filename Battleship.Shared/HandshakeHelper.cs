using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Battleship.Shared;

// Bắt tay (Handshake) 2 chiều ngay sau khi mở kết nối TCP:
// Client gửi HandshakeDto -> Server kiểm tra phiên bản giao thức -> trả HandshakeAckDto.
// Nhờ bước này, Client/Server lệch phiên bản sẽ bị từ chối ngay thay vì lỗi parse lắt nhắt về sau.
public static class HandshakeHelper
{
    // ----- Phía CLIENT -----
    // Gửi gói bắt tay rồi chờ Ack. Trả null nếu Server đóng kết nối giữa chừng.
    public static async Task<HandshakeAckDto?> ClientHandshakeAsync(
        NetworkStream stream, string clientVersion, string role = "player", CancellationToken ct = default)
    {
        var hello = new HandshakeDto
        {
            ProtocolVersion = ProtocolInfo.Version,
            ClientVersion = clientVersion,
            Role = role
        };

        await PacketFraming.SendPacketAsync(stream, Packet.Create(PacketType.Handshake, hello), ct);

        Packet? reply = await PacketFraming.ReadPacketAsync(stream, ct);
        if (reply == null || reply.Type != PacketType.Handshake)
            return null;

        return (HandshakeAckDto?)reply.DeserializePayload(typeof(HandshakeAckDto));
    }

    // ----- Phía SERVER -----
    // Đọc gói bắt tay của Client, kiểm tra phiên bản, gửi Ack và trả về true nếu chấp nhận.
    public static async Task<bool> ServerHandshakeAsync(
        NetworkStream stream, string sessionId, CancellationToken ct = default)
    {
        Packet? first = await PacketFraming.ReadPacketAsync(stream, ct);
        if (first == null || first.Type != PacketType.Handshake)
        {
            await SendAckAsync(stream, new HandshakeAckDto
            {
                Accepted = false,
                SessionId = sessionId,
                Message = "Gói bắt tay không hợp lệ."
            }, ct);
            return false;
        }

        var hello = (HandshakeDto?)first.DeserializePayload(typeof(HandshakeDto));
        bool versionOk = hello != null && hello.ProtocolVersion == ProtocolInfo.Version;

        await SendAckAsync(stream, new HandshakeAckDto
        {
            Accepted = versionOk,
            SessionId = sessionId,
            Message = versionOk
                ? "Kết nối thành công."
                : $"Lệch phiên bản giao thức. Server yêu cầu v{ProtocolInfo.Version}."
        }, ct);

        return versionOk;
    }

    private static Task SendAckAsync(NetworkStream stream, HandshakeAckDto ack, CancellationToken ct)
        => PacketFraming.SendPacketAsync(stream, Packet.Create(PacketType.Handshake, ack), ct);
}
