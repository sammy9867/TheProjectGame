using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class Board
    {

        /**Changing "Status" during Communication phase **/
        public enum Status
        {
            TASK_AREA,
            PIECE_AREA,
            RED_GOAL_AREA,
            BLUE_GOAL_AREA,
            RED_PLAYER,
            BLUE_PLAYER,
            UNDISCOVERED_RED_GOALS,
            UNDISCOVERED_BLUE_GOALS,
            DISCOVERED_RED_GOALS,
            DISCOVERED_BLUE_GOALS,
            DISCOVERED_NON_GOAL,
            RED_PLAYER_PIECE,
            BLUE_PLAYER_PIECE

        }

        public static int Width { get; set; }
        public static int Height { get; set; }
        public static int GoalHeight { get; set; }
        public static int TaskHeight { get; set; }

        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }
        public List<Piece> Pieces { get; set; }
        public List<Goal> Goals { get; set; }
        public double ProbabilityOfBeingSham { get; set; }
        public int FrequencyOfPlacingPieces { get; set; }
        public int InitialNumberOfPieces { get; set; }
        public int NumberOfGoals { get; set; }

        public int getCellStatus(int col, int row)
        {
            /* Check if PIECE occupaes a cell */
            foreach (var item in Pieces)
            {
                if (item.isTaken(col, row))
                    return (int)Board.Status.PIECE_AREA;
            }

            /* Check if goal */
            foreach (var item in Goals)
            {
                if (item.isTaken(col, row))
                    return (int)Board.Status.DISCOVERED_RED_GOALS;
            }

            /* Check if RED player occupaes a cell */
            if (RedTeam.isTaken(col, row) == 1)
                return (int)Board.Status.RED_PLAYER;
            else if (RedTeam.isTaken(col, row) == 2)
                return (int)Board.Status.RED_PLAYER_PIECE;

            /* Check if BLUE player occupaes a cell */
            if (BlueTeam.isTaken(col, row) == 1)
                return (int)Board.Status.BLUE_PLAYER;
            else if (BlueTeam.isTaken(col, row) == 2)
                return (int)Board.Status.BLUE_PLAYER_PIECE;

            
  
            /* Is goal RED area */
            if (row < GoalHeight)
                return (int)Board.Status.RED_GOAL_AREA;

            /* Is goal BLUE area */
            if (Height - row - 1< GoalHeight)
                return (int) Board.Status.BLUE_GOAL_AREA;


            return (int)Board.Status.TASK_AREA;
        }

    }
}
