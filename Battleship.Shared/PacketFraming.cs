using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Battleship.Shared;

public static class PacketFraming
{
    private const int MaxPacketSize = 10 * 1024 * 1024; // 10MB tối đa

    // Gửi gói tin qua NetworkStream: 4 bytes length + json body
    public static void SendPacket(NetworkStream stream, Packet packet)
    {
        string json = JsonSerializer.Serialize(packet, typeof(Packet));
        byte[] body = Encoding.UTF8.GetBytes(json);
        byte[] lengthHeader = BitConverter.GetBytes(body.Length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthHeader);

        stream.Write(lengthHeader, 0, 4);
        stream.Write(body, 0, body.Length);
        stream.Flush();
    }

    // Đọc gói tin qua NetworkStream
    public static Packet? ReadPacket(NetworkStream stream)
    {
        byte[] lengthHeader = new byte[4];
        if (!ReadExact(stream, lengthHeader, 0, 4))
            return null;

        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthHeader);

        int packetLength = BitConverter.ToInt32(lengthHeader, 0);
        if (packetLength <= 0 || packetLength > MaxPacketSize)
            throw new InvalidDataException("Kich thuoc goi tin khong hop le.");

        byte[] body = new byte[packetLength];
        if (!ReadExact(stream, body, 0, packetLength))
            return null;

        string json = Encoding.UTF8.GetString(body);
        return (Packet?)JsonSerializer.Deserialize(json, typeof(Packet));
    }

    private static bool ReadExact(NetworkStream stream, byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesRead = stream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
                return false;

            totalBytesRead += bytesRead;
        }
        return true;
    }

    // ===== Phiên bản BẤT ĐỒNG BỘ (dùng cho TCP Server async/await) =====

    // Gửi gói tin bất đồng bộ: 4 bytes length (big-endian) + json body.
    // Ghép header + body thành 1 frame rồi ghi 1 lần để 2 Task gửi song song
    // không xen kẽ byte của nhau trên cùng stream.
    public static async Task SendPacketAsync(NetworkStream stream, Packet packet, CancellationToken ct = default)
    {
        string json = JsonSerializer.Serialize(packet, typeof(Packet));
        byte[] body = Encoding.UTF8.GetBytes(json);
        byte[] lengthHeader = BitConverter.GetBytes(body.Length);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthHeader);

        byte[] frame = new byte[4 + body.Length];
        Buffer.BlockCopy(lengthHeader, 0, frame, 0, 4);
        Buffer.BlockCopy(body, 0, frame, 4, body.Length);

        await stream.WriteAsync(frame.AsMemory(0, frame.Length), ct);
        await stream.FlushAsync(ct);
    }

    // Đọc gói tin bất đồng bộ. Trả về null khi client đóng kết nối (EOF).
    public static async Task<Packet?> ReadPacketAsync(NetworkStream stream, CancellationToken ct = default)
    {
        byte[] lengthHeader = new byte[4];
        if (!await ReadExactAsync(stream, lengthHeader, 4, ct))
            return null;

        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthHeader);

        int packetLength = BitConverter.ToInt32(lengthHeader, 0);
        if (packetLength <= 0 || packetLength > MaxPacketSize)
            throw new InvalidDataException("Kich thuoc goi tin khong hop le.");

        byte[] body = new byte[packetLength];
        if (!await ReadExactAsync(stream, body, packetLength, ct))
            return null;

        string json = Encoding.UTF8.GetString(body);
        return (Packet?)JsonSerializer.Deserialize(json, typeof(Packet));
    }

    // Vòng lặp đọc cho tới khi đủ đúng 'count' byte theo PayloadLength,
    // chống hiện tượng dính gói / vỡ gói khi mạng lag.
    private static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken ct)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < count)
        {
            int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalBytesRead, count - totalBytesRead), ct);
            if (bytesRead == 0)
                return false;

            totalBytesRead += bytesRead;
        }
        return true;
    }
}