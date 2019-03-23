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
            BLUE_PLAYER_PIECE,
            END_OF_BOARD
        }

        public static int Width { get; set; }
        public static int Height { get; set; }
        public static int GoalHeight { get; set; }
        public static int TaskHeight { get; set; }

        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }
        public List<Piece> Pieces { get; set; }

        //Goals
        public List<Goal> UndiscoveredRedGoals { get; set; }
        public List<Goal> UndiscoveredBlueGoals { get; set; }
        public List<Goal> DiscoveredRedGoals { get; set; }
        public List<Goal> DiscoveredBlueGoals { get; set; }
        public List<Goal> NonGoals { get; set; }

        public double ProbabilityOfBeingSham { get; set; }
        public int FrequencyOfPlacingPieces { get; set; }
        public int InitialNumberOfPieces { get; set; }
        public int NumberOfGoals { get; set; }

        public int getCellStatus(int col, int row)
        {
            if ((col < 0) || (row < 0) || (row >= Height) || (col >= Width))
                return (int)Status.END_OF_BOARD;

            /* Check if PIECE occupaes a cell */
            foreach (var item in Pieces)
            {
                if (item.isTaken(col, row))
                    return (int)Board.Status.PIECE_AREA;
            }


            /* Checks if GOAL belongs to a SPECIFIC type of goal */
            #region Goal

            foreach (var item in UndiscoveredRedGoals)
            {
                
                if(item.isTaken(col,row))
                {
                    return (int)Board.Status.UNDISCOVERED_RED_GOALS;
                }
            }

            foreach (var item in UndiscoveredBlueGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.UNDISCOVERED_BLUE_GOALS;
                }
            }

            foreach (var item in DiscoveredRedGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.DISCOVERED_RED_GOALS;
                }
            }

            foreach (var item in DiscoveredBlueGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.DISCOVERED_BLUE_GOALS;
                }
            }

            foreach (var item in NonGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.DISCOVERED_NON_GOAL;
                }
            }

            #endregion


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

        internal Player.NeighborStatus GetPlayersNeighbor(int column, int row, Team.TeamColor team)
        {
            /* End of the board */
            if ((column < 0) || (row < 0) || (row >= Height) || (column >= Width))
                return Player.NeighborStatus.BLOCKED;

            /* Goal Area */
            if (row < GoalHeight)
            {
                if (team == Team.TeamColor.RED)
                    return Player.NeighborStatus.GOAL_AREA;
                else return Player.NeighborStatus.BLOCKED;
            }
            if (Height - row - 1 < GoalHeight)
            {
                if (team == Team.TeamColor.BLUE)
                    return Player.NeighborStatus.GOAL_AREA;
                else return Player.NeighborStatus.BLOCKED;
            }

            /* Other player */
            foreach (Player p in RedTeam.members)
                if (p.column == column && p.row == row)
                    return Player.NeighborStatus.BLOCKED;
            if (RedTeam.leader.column == column && RedTeam.leader.row == row)
                return Player.NeighborStatus.BLOCKED;

            foreach (Player p in BlueTeam.members)
                if (p.column == column && p.row == row)
                    return Player.NeighborStatus.BLOCKED;
            if (BlueTeam.leader.column == column && BlueTeam.leader.row == row)
                return Player.NeighborStatus.BLOCKED;

            /* Piece */
            foreach (Piece p in Pieces)
                if (p.column == column && p.row == row)
                    return Player.NeighborStatus.PIECE;

            /* Free cell */
            return Player.NeighborStatus.FREE;
        }
    }
}
