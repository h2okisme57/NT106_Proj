using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Client.Models
{
    internal class MatchDetail
    {
        public int Id { get; set; }

        public int MatchId { get; set; }
        public Match Match { get; set; }

        public int PlayerId { get; set; }
        public User Player { get; set; }

        public int X_Coordinate { get; set; }
        public int Y_Coordinate { get; set; }
        public bool IsHit { get; set; }

        public DateTime TurnTime { get; set; } = DateTime.Now;
    }
}
