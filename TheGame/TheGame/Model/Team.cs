using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Team
    {
        public enum TeamColor { blue, red };

        public Player leader;
        public List<Player> members;
        public TeamColor teamColor { get; set; }

        int maxNumOfPlayers { get; set; }
        
        
    }

}
