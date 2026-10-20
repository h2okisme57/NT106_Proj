using System;
using System.Text.Json;

namespace Battleship.Shared;

// LƯU Ý CHUNG: chỉ được THÊM giá trị mới vào CUỐI enum, không chèn giữa / đổi thứ tự.
// (Đã bật JsonStringEnumConverter nên serialize theo tên, nhưng vẫn giữ nếp append cho sạch.)
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
    Error,
    // ----- Bổ sung cho các tuần tới (append cuối) -----
    KickPlayerRequest,          // Đức - 3.4
    MatchmakingJoinRequest,     // Hoàng - 3.6
    MatchmakingCancelRequest,   // Hoàng - 3.6
    MatchFound,                 // Hoàng - 3.6
    OnlineListUpdate,           // Dũng - 3.2 (broadcast Online/Offline)
    GetFriendsRequest,
    GetFriendsResponse,
    JoinRoomResponse,
    Ping,                       // Hoàng - 4.3 (heartbeat)
    Pong                        // Hoàng - 4.3 (heartbeat)
}

public class Packet
{
    public PacketType Type { get; set; }
    public string Payload { get; set; } = string.Empty;

    // Đóng gói: serialize data -> chuỗi JSON đặt vào Payload (dùng cấu hình JSON chung).
    public static Packet Create(PacketType type, object data)
    {
        return new Packet
        {
            Type = type,
            Payload = JsonSerializer.Serialize(data, JsonConfig.Options)
        };
    }

    // Mở gói: deserialize Payload về đúng kiểu DTO.
    public object? DeserializePayload(Type targetType)
    {
        if (string.IsNullOrWhiteSpace(Payload))
            return default;

        return JsonSerializer.Deserialize(Payload, targetType, JsonConfig.Options);
    }

    // Tiện ích generic cho gọn ở phía xử lý.
    public T? DeserializePayload<T>()
    {
        if (string.IsNullOrWhiteSpace(Payload))
            return default;

        // BỔ SUNG PHẦN BỊ THIẾU Ở ĐÂY
        return JsonSerializer.Deserialize<T>(Payload, JsonConfig.Options);
    }
}





// Gói lỗi chung, Server trả khi xử lý thất bại / chưa hỗ trợ.
public class ErrorDto
{
    public string Message { get; set; } = string.Empty;
}

// ===== Phiên bản giao thức & Bắt tay (Handshake) =====

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
