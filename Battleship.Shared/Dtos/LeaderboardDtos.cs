namespace Battleship.Shared;

// 1 dòng trong bảng xếp hạng, khớp với các cột của view "leaderboard" trên Supabase.
// Tên thuộc tính phải trùng tên cột (Username, Wins, Losses, Score) để đọc JSON cho đúng.
public class LeaderboardEntryDto
{
    public string Username { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Score { get; set; }
}
