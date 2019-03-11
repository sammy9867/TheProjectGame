using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Board
    {
        public enum Status
        {
            SIMPE_AREA,
            RED_GOAL,
            BLUE_GOAL,
            RED_PLAYER,
            BLUE_PLAYER
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int GoalWidth { get; set; }
        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }

        public int getCellStatus(int col, int row)
        {
            /* Check if RED player occupaes a cell */
            if (RedTeam.isTaken(col, row))
                return (int) Board.Status.RED_PLAYER;

            /* Check if BLUE player occupaes a cell */
            if (BlueTeam.isTaken(col, row))
                return (int)Board.Status.BLUE_PLAYER;

            /* Is goal BLUE area */
            if (Height - row -1 < GoalWidth)
                return (int) Board.Status.BLUE_GOAL;

            /* Is goal RED area */
            if (row < GoalWidth)
                return (int)Board.Status.RED_GOAL;

            return (int) Board.Status.SIMPE_AREA;
        }

    }
}
