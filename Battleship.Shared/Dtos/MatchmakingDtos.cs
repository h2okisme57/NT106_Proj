namespace Battleship.Shared;

// Server gửi cái này cho 2 người khi tìm được đối thủ
public class MatchFoundDto
{
    public string RoomId { get; set; } = string.Empty;       // phòng server vừa tạo cho 2 người
    public string OpponentName { get; set; } = string.Empty; // tên đối thủ
    public bool IsHost { get; set; }                         // true = người đứng đầu hàng đợi (làm chủ phòng)
}
