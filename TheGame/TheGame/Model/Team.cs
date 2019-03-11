using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Team
    {
        public Player leader;
        public List<Player> members;
        int maxNumOfPlayers { get; set; }
        public enum TeamColor { blue, red }
        
    }

}
