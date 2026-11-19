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

    // Email dùng để lấy lại mật khẩu khi quên
    public string Email { get; set; } = string.Empty;
}

public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// ===== Quên mật khẩu: bước 1 xin mã, bước 2 đặt mật khẩu mới =====

public class ForgotPasswordRequestDto
{
    public string Username { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;          // mã 6 số nhận qua email
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

// ===== Đăng xuất (Logout) =====
public class LogoutRequestDto
{
    public string Username { get; set; } = string.Empty;
}

public class LogoutResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}


