using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Team
    {
        public enum TeamColor { BLUE, RED };

        public Player leader;
        public List<Player> members;
        public TeamColor teamColor { get; set; }

        public int maxNumOfPlayers { get; set; }

        internal bool isTaken(int col, int row)
        {
            if (leader.column == col && leader.row == row)
                return true;

            foreach (var item in members)
            {
                if (item.row == row && item.column == col)
                    return true;
            }

            return false;
        }
    }

}
