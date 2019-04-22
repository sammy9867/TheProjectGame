using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class Player
    {
        public enum Role { MEMBER, LEADER }
        public enum NeighborStatus
        {
            FREE = 0,               // 0000
            PIECE = 1,              // 0001

            GOAL_AREA = 4,          // 0100
            DISCOVERED_NONGOAL = 6, // 0110
            DISCOVERED_GOAL = 7,    // 0111

            BLOCKED = 8,            // 1000
        }

        public NeighborStatus[,] Neighbors { get; set; }  // [column, row]
        public string playerID { get; set; }

        public int Row { get; set; }
        public int Column { get; set; }
        public int X { get { return Column; } }
        public int Y { get { return Row; } }

        public Role role { get; set; }
        public Piece Piece { get; set; }

        public Team.TeamColor Team { get; set; }

        private enum AlternativeStep { UP, DOWN, LEFT, RIGHT }

        private AlternativeStep GoalStep = AlternativeStep.LEFT;

        private bool toCheck;

        #region MOVES
        /**
         *  Player moves Up
         *  @return 0 on sucess, -1 otherwise
         */
        public int goUp()
        {
            if (Team == Model.Team.TeamColor.RED && Row == 0)
                return -1;
            if (Team == Model.Team.TeamColor.BLUE && Row == Board.GoalHeight)
                return -1;

            Row--;
            return 0;
        }

        /**
         *  Player moves Down
         *  @return 0 on sucess, -1 otherwise
         */
        public int goDown()
        {
            if (Team == Model.Team.TeamColor.BLUE && Row == Board.Height - 1)
                return -1;
            if (Team == Model.Team.TeamColor.RED && Row == Board.Height - Board.GoalHeight - 1)
                return -1;

            Row++;
            return 0;
        }
            
        /**
         *  Player moves Left
         *  @return 0 on sucess, -1 otherwise
         */
        public int goLeft()
        {
            if (Column == 0)
                return -1;
            Column--;
            return 0;
        }

        /**
         *  Player moves Right
         *  @return 0 on sucess, -1 otherwise
         */
        public int goRight()
        {
            if (Board.Width - 1 == Column)
                return -1;
            Column++;
            return 0;
        }
        #endregion

        /**
         *  Player moves Randomly
         *  @return 0 on sucess, -1 otherwise
         */
        public int goRnd()
        {
            if (toCheck)
            {
                if (Piece.isSham)   // if a piece is a sham
                    Piece = null;   // destroy it, by simply forgetting 
                toCheck = false;
                return 0;
            }

            // [column , row]

            #region Go With a Piece
            if (hasPiece())
            {
                if (Team == Model.Team.TeamColor.RED)
                {
                    if(Neighbors[1,0] != NeighborStatus.BLOCKED &&
                        (Neighbors[1, 1] & NeighborStatus.GOAL_AREA) != NeighborStatus.GOAL_AREA)
                        return goUp();
                    return goForGoalAlternative(Model.Team.TeamColor.RED);

                }
                else
                {
                    if (Neighbors[1, 2] != NeighborStatus.BLOCKED &&
                        (Neighbors[1,1]&NeighborStatus.GOAL_AREA) != NeighborStatus.GOAL_AREA)
                        return goDown();
                    return goForGoalAlternative(Model.Team.TeamColor.BLUE);
                }
            }
            #endregion
            
            #region Go Back to the Task Area
            if ((Neighbors[1, 1] & NeighborStatus.GOAL_AREA) == NeighborStatus.GOAL_AREA)
            {
                if (Team == Model.Team.TeamColor.RED && Neighbors[1,2] != NeighborStatus.BLOCKED)
                {
                    if (Neighbors[1, 2] != NeighborStatus.BLOCKED)
                        return goDown();
                    if (Neighbors[2, 1] != NeighborStatus.BLOCKED)
                        return goRight();
                    if (Neighbors[0, 1] != NeighborStatus.BLOCKED)
                        return goLeft();
                    if (Neighbors[1, 0] != NeighborStatus.BLOCKED)
                        return goUp();
                }
                else if (Neighbors[1,0] != NeighborStatus.BLOCKED)
                {
                    if (Neighbors[1, 0] != NeighborStatus.BLOCKED)
                        return goUp();

                    if (Neighbors[2, 1] != NeighborStatus.BLOCKED)
                        return goRight();
                    if (Neighbors[0, 1] != NeighborStatus.BLOCKED)
                        return goLeft();
                    if (Neighbors[1, 2] != NeighborStatus.BLOCKED)
                        return goDown(); ;
                }
            }
            #endregion
          
            #region To Neighbouring piece
            if (Neighbors[0, 1] == NeighborStatus.PIECE) return goLeft();
            if (Neighbors[1, 0] == NeighborStatus.PIECE) return goUp();
            if (Neighbors[2, 1] == NeighborStatus.PIECE) return goRight();
            if (Neighbors[1, 2] == NeighborStatus.PIECE) return goDown();
            
            if (Neighbors[0, 0] == NeighborStatus.PIECE) return goLeft();
            if (Neighbors[2, 0] == NeighborStatus.PIECE) return goUp();
            if (Neighbors[2, 2] == NeighborStatus.PIECE) return goRight();
            if (Neighbors[0, 2] == NeighborStatus.PIECE) return goDown();
            #endregion
         
            #region Go Random
            /* Go Random */
            Random r = new Random(Guid.NewGuid().GetHashCode());
            while (true)
            {
                switch (r.Next() % 4)
                {
                    case 0: if (Neighbors[1, 0] != NeighborStatus.BLOCKED) return goUp(); else break;
                    case 1: if (Neighbors[1, 2] != NeighborStatus.BLOCKED) return goDown(); else break;
                    case 2: if (Neighbors[0, 1] != NeighborStatus.BLOCKED) return goLeft(); else break;
                    case 3: if (Neighbors[2, 1] != NeighborStatus.BLOCKED) return goRight(); else break;
                }
            }
#endregion
        }

        private int goForGoalAlternative(Team.TeamColor color)
        {

            while(true)
            switch (GoalStep)
            {
            case AlternativeStep.LEFT:
                if (Neighbors[0, 1] != NeighborStatus.BLOCKED)
                    return goLeft();
                GoalStep = AlternativeStep.RIGHT;
                break;

            case AlternativeStep.RIGHT:
                if (Neighbors[2, 1] != NeighborStatus.BLOCKED)
                    return goRight();

                if (color == Model.Team.TeamColor.RED)
                    GoalStep = AlternativeStep.UP;
                else
                    GoalStep = AlternativeStep.DOWN;
                break;

            case AlternativeStep.UP:
                goUp();
                GoalStep = AlternativeStep.LEFT;
                return 0;

            case AlternativeStep.DOWN:
                goDown();
                GoalStep = AlternativeStep.LEFT;
                return 0;
            }
        }

        internal void checkPiece()
        {
            if (Piece == null) return;
            toCheck = true;
        }

        public bool hasPiece()
        {
            if (null == Piece)
                return false;
            return true;
        }
    }
}
