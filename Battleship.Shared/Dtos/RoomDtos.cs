using System.Collections.Generic;

namespace Battleship.Shared;

// ===== Phòng đấu & Sảnh chờ (dùng cho phần của Đức - 3.4) =====

public enum RoomState
{
    Waiting,   // đang chờ đủ người
    Playing,   // đang trong trận
    Finished   // đã kết thúc
}

public class CreateRoomDto
{
    public string RoomId { get; set; } = string.Empty;   // để trống = Server tự sinh ID
    public string RoomName { get; set; } = string.Empty;
}

public class JoinRoomDto
{
    public string RoomId { get; set; } = string.Empty;
}

public class LeaveRoomDto
{
    public string RoomId { get; set; } = string.Empty;
}

public class KickPlayerDto
{
    public string RoomId { get; set; } = string.Empty;
    public string TargetUsername { get; set; } = string.Empty;
}
// Lấy danh sách phòng hiện có trong sảnh chờ.
public class JoinRoomResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    // Nếu thành công, trả về luôn thông tin phòng mới nhất để Client vẽ giao diện
    public RoomInfoDto? Room { get; set; }
}

// Thông tin tóm tắt 1 phòng (dùng cho danh sách phòng và cập nhật phòng).
public class RoomInfoDto
{
    public string RoomId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string? GuestName { get; set; }
    public RoomState State { get; set; } = RoomState.Waiting;
}

public class RoomUpdateDto
{
    public RoomInfoDto Room { get; set; } = new RoomInfoDto();
}

public class RoomListResponseDto
{
    public List<RoomInfoDto> Rooms { get; set; } = new List<RoomInfoDto>();
}
