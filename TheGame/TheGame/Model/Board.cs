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
            // 5th bit stays for BLOCKED, 4th bit stays for goal, 3 bit for goal cell
 /*0000 0000*/  TASK_CELL,   // ALWAYS THE FIRST ELEMENT 
 /*0000 0010*/  PIECE = 2,
 /*0000 0011*/  SHAM = 3,
 /*0000 0100*/  RED_GOALS_CELL,  // red above !!!
 /*0000 0100*/  BLUE_GOALS_CELL, // blue below!!!
 /*0001 0000*/  RED_PLAYER,
 /*0001 0000*/  BLUE_PLAYER,
 /*0000 1100*/  UNDISCOVERED_GOAL=8,
 /*0000 1101*/  DISCOVERED_GOAL=9,
 /*0000 0100*/  DISCOVERED_NON_GOAL,
 /*0001 0000*/  RED_PLAYER_WITH_PIECE,
 /*0001 0000*/  BLUE_PLAYER_WITH_PIECE,
 /*0001 0000*/  RED_PLAYER_AND_PIECE,        
 /*0001 0000*/  BLUE_PLAYER_AND_PIECE,
 /*0001 0000*/  RED_PLAYER_AND_SHAM,
 /*0001 0000*/  BLUE_PLAYER_AND_SHAM,
 /*0001 0000*/  END_OF_BOARD,

 /*0001 0000*/  BLOCKED = 16
        }

        public static int RedScore { get; set; }
        public static int BlueScore { get; set; }
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static int GoalHeight { get; set; }
        public static int TaskHeight { get; set; }
        public static int MaxNumOfPlayers { get; set; }
        public static double ProbabilityOfBeingSham { get; set; }
        public static int FrequencyOfPlacingPieces { get; set; }
        public static int InitialNumberOfPieces { get; set; }
        public static int NumberOfGoals { get; set; }
        public static int ShamProbability { get; set; }

        public Team BlueTeam { get; set; }
        public Team RedTeam { get; set; }
        public List<Piece> Pieces { get; set; }

        //Goals
        public List<Goal> UndiscoveredRedGoals { get; set; }
        public List<Goal> UndiscoveredBlueGoals { get; set; }
        public List<Goal> DiscoveredRedGoals { get; set; }
        public List<Goal> DiscoveredBlueGoals { get; set; }
        public List<Goal> NonGoals { get; set; }

        public Status[,] boardtable; // column row
        

        public int getCellStatus(int col, int row)
        {
            if ((col < 0) || (row < 0) || (row >= Height) || (col >= Width))
                return (int)Status.END_OF_BOARD;

            #region Player Occupation
            /* Check if RED player occupaes a cell */
            if (RedTeam.isTaken(col, row) == Team.TeamCell.PLAYER)
                return (int)Board.Status.RED_PLAYER;
            else if (RedTeam.isTaken(col, row) == Team.TeamCell.PLAYER_PIECE)
                return (int)Board.Status.RED_PLAYER_WITH_PIECE;

            /* Check if BLUE player occupaes a cell */
            if (BlueTeam.isTaken(col, row) == Team.TeamCell.PLAYER)
                return (int)Board.Status.BLUE_PLAYER;
            else if (BlueTeam.isTaken(col, row) == Team.TeamCell.PLAYER_PIECE)
                return (int)Board.Status.BLUE_PLAYER_WITH_PIECE;
            #endregion

            #region Check if PIECE occupaes a cell 
            /* Check if PIECE occupaes a cell */
            foreach (var item in Pieces)
            {
                if (item.isTaken(col, row))
                {
                    if (item.isSham) return (int)Board.Status.SHAM;
                    else return (int)Board.Status.PIECE;
                }
            }
            #endregion

            #region Goal
            /* Checks if GOAL belongs to a SPECIFIC type of goal */
            foreach (var item in UndiscoveredRedGoals)
            {
                
                if(item.isTaken(col,row))
                {
                    return (int)Board.Status.UNDISCOVERED_GOAL;
                }
            }

            foreach (var item in UndiscoveredBlueGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.UNDISCOVERED_GOAL;
                }
            }

            foreach (var item in DiscoveredRedGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.DISCOVERED_GOAL;
                }
            }

            foreach (var item in DiscoveredBlueGoals)
            {

                if (item.isTaken(col, row))
                {
                    return (int)Board.Status.DISCOVERED_GOAL;
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

            /* Is goal RED area */
            if (row < GoalHeight)
            {
                Team.TeamCell teamCell = RedTeam.isDiscovered(col, row);
                if (teamCell == Team.TeamCell.FREE) return (int)Board.Status.RED_GOALS_CELL;
                if (teamCell == Team.TeamCell.DISCOVERED_GOAL) return (int)Board.Status.DISCOVERED_GOAL;
                if (teamCell == Team.TeamCell.DISCOVERED_NONGOAL) return (int)Board.Status.DISCOVERED_NON_GOAL;
            }
            /* Is goal BLUE area */
            if (Height - row - 1 < GoalHeight)
            {
                Team.TeamCell teamCell = BlueTeam.isDiscovered(col, row);
                if (teamCell == Team.TeamCell.FREE) return (int)Board.Status.BLUE_GOALS_CELL;
                if (teamCell == Team.TeamCell.DISCOVERED_GOAL) return (int)Board.Status.DISCOVERED_GOAL;
                if (teamCell == Team.TeamCell.DISCOVERED_NONGOAL) return (int)Board.Status.DISCOVERED_NON_GOAL;
            }
            
            return (int)Board.Status.TASK_CELL;
        }

        internal bool IsUndiscoveredGoal(int c, int r)
        {
            foreach (Goal goal in UndiscoveredRedGoals)
                if (goal.column == c && goal.row == r)
                    return true;

            foreach (Goal goal in UndiscoveredBlueGoals)
                if (goal.column == c && goal.row == r)
                    return true;

            return false;
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
                {
                    Team.TeamCell teamCell = RedTeam.isDiscovered(column, row);
                    if(teamCell == Team.TeamCell.FREE) return Player.NeighborStatus.GOAL_AREA;
                    if (teamCell == Team.TeamCell.DISCOVERED_GOAL) return Player.NeighborStatus.DISCOVERED_GOAL;
                    if (teamCell == Team.TeamCell.DISCOVERED_NONGOAL) return Player.NeighborStatus.DISCOVERED_NONGOAL;
                }
                else return Player.NeighborStatus.BLOCKED;
            }
            if (Height - row - 1 < GoalHeight)
            {
                if (team == Team.TeamColor.BLUE)
                {
                    Team.TeamCell teamCell = BlueTeam.isDiscovered(column, row);
                    if (teamCell == Team.TeamCell.FREE) return Player.NeighborStatus.GOAL_AREA;
                    if (teamCell == Team.TeamCell.DISCOVERED_GOAL) return Player.NeighborStatus.DISCOVERED_GOAL;
                    if (teamCell == Team.TeamCell.DISCOVERED_NONGOAL) return Player.NeighborStatus.DISCOVERED_NONGOAL;
                }
                else return Player.NeighborStatus.BLOCKED;
            }

            /* Other player */
            foreach (Player p in RedTeam.members)
                if (p.Column == column && p.Row == row)
                    return Player.NeighborStatus.BLOCKED;
            if (RedTeam.leader.Column == column && RedTeam.leader.Row == row)
                return Player.NeighborStatus.BLOCKED;

            foreach (Player p in BlueTeam.members)
                if (p.Column == column && p.Row == row)
                    return Player.NeighborStatus.BLOCKED;
            if (BlueTeam.leader.Column == column && BlueTeam.leader.Row == row)
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
