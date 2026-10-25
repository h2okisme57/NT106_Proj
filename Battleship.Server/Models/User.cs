using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client.Models
{
    public class User
    {
        // Id kiểu string nên EF không tự sinh giá trị, phải tự gán (dùng Guid cho không trùng)
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public bool Condition { get; set; } = false;
        public string Username { get; set; }
        public string Password { get; set; }
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Score { get; set; } = 0;
        public ICollection<User> Friend { get; set; }
        public ICollection<MatchDetail> MatchDetails { get; set; }
    }
}
