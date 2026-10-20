namespace Battleship.Shared;

// ===== Tài khoản: Đăng nhập / Đăng ký (dùng cho phần của Dũng - 3.2) =====

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // --- Toàn bộ thông tin người chơi ---
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Score { get; set; }
         public List<string> FriendsList { get; set; } = new List<string>();
    
}

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
