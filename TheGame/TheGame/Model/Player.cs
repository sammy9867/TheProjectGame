using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Player
    {
        public enum Role { member, leader }
        public int playerID { get; set; }
        public int row { get; set; }
        public int columns { get; set; }
        public Role role { get; set; }
    }
}
