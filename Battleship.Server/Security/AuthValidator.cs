using System.Text.RegularExpressions;

namespace Battleship.Server;

// Kiểm tra tên đăng nhập và mật khẩu mà client gửi lên.
// Không tin dữ liệu từ client: gói tin có thể bị tự viết tay, nên server phải tự kiểm tra lại.
public static class AuthValidator
{
    // Tên: 3 đến 20 ký tự, chỉ gồm chữ không dấu, số và dấu gạch dưới
    private static readonly Regex UsernameRegex = new Regex("^[A-Za-z0-9_]{3,20}$");

    public const int MinPasswordLength = 6;
    public const int MaxPasswordLength = 64;   // giới hạn trên để không ai gửi mật khẩu dài cả triệu ký tự bắt BCrypt tính

    // Trả về null nếu hợp lệ, ngược lại trả về câu thông báo lỗi để gửi cho client
    public static string? CheckUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "Tên đăng nhập không được để trống.";

        if (!UsernameRegex.IsMatch(username))
            return "Tên đăng nhập dài 3-20 ký tự, chỉ gồm chữ không dấu, số và dấu gạch dưới (_).";

        return null;
    }

    public static string? CheckPassword(string? password, string username)
    {
        if (string.IsNullOrEmpty(password))
            return "Mật khẩu không được để trống.";

        if (password.Length < MinPasswordLength)
            return "Mật khẩu phải có ít nhất " + MinPasswordLength + " ký tự.";

        if (password.Length > MaxPasswordLength)
            return "Mật khẩu tối đa " + MaxPasswordLength + " ký tự.";

        if (string.Equals(password, username, StringComparison.OrdinalIgnoreCase))
            return "Mật khẩu không được trùng với tên đăng nhập.";

        return null;
    }
}
