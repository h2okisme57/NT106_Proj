using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client.Models
{
    public class Match
    {
        public int Id { get; set; }

        // Khóa ngoại phải cùng kiểu với User.Id (string)
        public string Player1Id { get; set; }
        public User Player1 { get; set; }

        public string? Player2Id { get; set; }
        public User Player2 { get; set; }

        public string? WinnerId { get; set; }
        public User Winner { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        public ICollection<MatchDetail> MatchDetails { get; set; }
    }
}
