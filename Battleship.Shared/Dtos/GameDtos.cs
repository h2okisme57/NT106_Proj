using System.Collections.Generic;

namespace Battleship.Shared;

// ===== Gameplay turn-based (dùng cho phần của Đức - 4.2 và UI của Dũng - 4.1/4.4) =====

// Một con tàu khi đặt lên lưới 10x10.
public class ShipDto
{
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }                 // tọa độ ô đầu (0..9)
    public int Y { get; set; }
    public int Length { get; set; }
    public bool IsHorizontal { get; set; } = true;
}

// Client gửi toàn bộ vị trí tàu đã xếp.
public class PlaceShipsDto
{
    public List<ShipDto> Ships { get; set; } = new List<ShipDto>();
}

public class PlayerReadyDto
{
    public bool Ready { get; set; } = true;
}

public class GameStartDto
{
    public string FirstTurnPlayer { get; set; } = string.Empty;
}

// Client bắn 1 ô.
public class FireShotDto
{
    public int X { get; set; }
    public int Y { get; set; }
}

// Server phán kết quả 1 phát bắn.
public class ShotResultDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public string? SunkShipName { get; set; }
    public string ShooterName { get; set; } = string.Empty;   // ai vừa bắn
    public string NextTurnPlayer { get; set; } = string.Empty;
}

public class TurnChangeDto
{
    public string CurrentTurnPlayer { get; set; } = string.Empty;
    public int SecondsLeft { get; set; } = 30;   // phục vụ Turn Timeout 30s (4.3)
}

public class GameOverDto
{
    public string Winner { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;   // "AllShipsSunk" | "Disconnected" | "Timeout"
}
