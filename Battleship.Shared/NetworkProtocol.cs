using System;
using System.Text.Json;

namespace Battleship.Shared;

public enum PacketType
{
    Handshake,
    LoginRequest,
    LoginResponse,
    RegisterRequest,
    RegisterResponse,
    GetRoomListRequest,
    GetRoomListResponse,
    CreateRoomRequest,
    JoinRoomRequest,
    RoomUpdate,
    LeaveRoom,
    PlaceShipsRequest,
    PlayerReady,
    GameStart,
    FireShotRequest,
    ShotResult,
    TurnChange,
    GameOver,
    ChatMessage,
    OpponentDisconnected,
    Error
}

public class Packet
{
    public PacketType Type { get; set; }
    public string Payload { get; set; } = string.Empty;

    public static Packet Create(PacketType type, object data)
    {
        return new Packet
        {
            Type = type,
            Payload = JsonSerializer.Serialize(data)
        };
    }

    public object? DeserializePayload(Type targetType)
    {
        if (string.IsNullOrWhiteSpace(Payload))
            return default;

        return JsonSerializer.Deserialize(Payload, targetType);
    }
}

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class FireShotDto
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class ShotResultDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public string? SunkShipName { get; set; }
    public string NextTurnPlayer { get; set; } = string.Empty;
}

public class ChatMessageDto
{
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

// Thông tin phiên bản giao thức dùng chung Client - Server.
// Tăng số này mỗi khi đổi cấu trúc Packet/DTO để 2 bên từ chối nối nhầm phiên bản.
public static class ProtocolInfo
{
    public const int Version = 1;
}

// Gói tin bắt tay Client gửi ngay sau khi kết nối (trước mọi gói khác).
public class HandshakeDto
{
    public int ProtocolVersion { get; set; } = ProtocolInfo.Version;
    public string ClientVersion { get; set; } = string.Empty;
    public string Role { get; set; } = "player"; // "player" hoặc "spectator"
}

// Server phản hồi kết quả bắt tay.
public class HandshakeAckDto
{
    public bool Accepted { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int ServerProtocolVersion { get; set; } = ProtocolInfo.Version;
}