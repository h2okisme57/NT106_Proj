using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Battleship.Server.Helpers
{
    internal class PasswordHelper
    {
        public static string HashPassword(string plainPassword)
        {
            // Có thể tùy chỉnh độ khó (Work Factor) ở đây nếu muốn. Mặc định là 11.
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }

        // Hàm dùng để kiểm tra mật khẩu khi Đăng nhập
        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }
    }
}
