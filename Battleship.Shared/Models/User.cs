using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client.Models
{
    internal class User
    {
        public string Id { get; set; }
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
