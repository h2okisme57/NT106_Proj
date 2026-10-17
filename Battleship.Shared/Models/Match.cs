using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client.Models
{
    internal class Match
    {
        public int Id { get; set; }

        public int Player1Id { get; set; }
        public User Player1 { get; set; }

        public int? Player2Id { get; set; }
        public User Player2 { get; set; }

        public int? WinnerId { get; set; }
        public User Winner { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        public ICollection<MatchDetail> MatchDetails { get; set; }
    }
}
