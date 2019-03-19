using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class Team
    {
        public enum TeamColor { BLUE, RED };

        public Player leader;
        public List<Player> members;
        public TeamColor teamColor { get; set; }

        public int maxNumOfPlayers { get; set; }

        //internal bool
        public int isTaken(int col, int row)
        {
            if (leader.column == col && leader.row == row)
            {
                if (leader.hasPiece)
                    return 2;
                else return 1;
            }

            foreach (var item in members)
            {
                if (item.row == row && item.column == col)
                {
                    if (item.hasPiece) return 2;
                    else return 1;
                }
            }

            return 0;
        }
    }

}
