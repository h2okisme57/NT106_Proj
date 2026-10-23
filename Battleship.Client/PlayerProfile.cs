using Battleship.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client
{
    public static class PlayerProfile
    {
        // Gán thẳng nguyên cái hộp DTO vào đây!
        public static LoginResponseDto? CurrentUser { get; set; }

        // Các thuộc tính lấy nhanh dữ liệu cho giao diện
        public static string Id => CurrentUser?.UserId ?? string.Empty;
        public static string Username => CurrentUser?.Username ?? string.Empty;
        public static int Wins => CurrentUser?.Wins ?? 0;
        public static int Losses => CurrentUser?.Losses ?? 0;
        public static int Score => CurrentUser?.Score ?? 0;

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
