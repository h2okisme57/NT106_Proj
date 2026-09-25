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
}