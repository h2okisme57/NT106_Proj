using System.Collections.Generic;

namespace Battleship.Shared;

// 1. Kick người chơi khỏi phòng chờ (Mục 4.0)


// 2. Thông tin 1 con tàu trên lưới 10x10 (Mục 4.1 & 4.2)
public class ShipPlacementDto
{
    public string ShipName { get; set; } = string.Empty; // Carrier, Battleship, Cruiser, Submarine, Destroyer
    public int Size { get; set; }                        // 5, 4, 3, 3, 2
    public int StartX { get; set; }                      // 0..9 (Cột)
    public int StartY { get; set; }                      // 0..9 (Hàng)
    public bool IsHorizontal { get; set; } = true;       // true: ngang, false: dọc
}

// 3. Client gửi toàn bộ đội hình tàu đã xếp (PlaceShips)
public class PlaceShipsDto
{
    public string RoomId { get; set; } = string.Empty;
    public List<ShipPlacementDto> Ships { get; set; } = new();
}

// 4. Client báo sẵn sàng (PlayerReady)
public class PlayerReadyDto
{
    public string RoomId { get; set; } = string.Empty;
    public bool IsReady { get; set; } = true;
}

// 5. Server báo trận đấu chính thức bắt đầu (GameStart)
public class GameStartDto
{
    public string RoomId { get; set; } = string.Empty;
    public string FirstTurnUsername { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; } = 30;
}

// 6. Client gửi tọa độ bắn (FireShot)
public class FireShotDto
{
    public string RoomId { get; set; } = string.Empty;
    public int X { get; set; } // 0..9
    public int Y { get; set; } // 0..9
}

// 7. Server trả kết quả phát bắn (ShotResult)
public class ShotResultDto
{
    public string RoomId { get; set; } = string.Empty;
    public string ShooterUsername { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsValid { get; set; } = true;
    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public string? SunkShipName { get; set; }
    public string NextTurnUsername { get; set; } = string.Empty; // Trúng = ShooterUsername, Hụt = Đối thủ
}

// 8. Server thông báo đổi lượt / đếm ngược 30s (TurnChanged - Mục 4.3)
public class TurnChangedDto
{
    public string RoomId { get; set; } = string.Empty;
    public string CurrentTurnUsername { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; } = 30;
    public string Reason { get; set; } = "Normal"; // "GameStart", "Miss", "Timeout"
}

// 9. Server thông báo kết thúc trận đấu (GameOver - Mục 4.4 & 4.5)
public class GameOverDto
{
    public string RoomId { get; set; } = string.Empty;
    public string WinnerUsername { get; set; } = string.Empty;
    public string LoserUsername { get; set; } = string.Empty;
    public string Reason { get; set; } = "AllShipsSunk"; // "AllShipsSunk", "OpponentDisconnected", "Surrender", "Timeout"
}
