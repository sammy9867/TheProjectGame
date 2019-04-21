using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePlayers
{
    public class Player
    {
        private const int SUCCESS =  0;
        private const int FAILURE = -1;

        public enum Role { MEMBER, LEADER }
        public enum TeamColor { BLUE, RED };
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
        public Role role { get; set; }
        public bool hasPiece { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }

        public int X { get { return Column; }  }
        public int Y { get { return Row; } }


        public TeamColor Team { get; set; }

        private enum AlternativeStep { UP, DOWN, LEFT, RIGHT }

        private AlternativeStep GoalStep = AlternativeStep.LEFT;

        private bool toCheck;

        #region MOVES
       public int goUp()
        {
            // [column , row]
            if (Neighbors[1, 0] == NeighborStatus.BLOCKED)
                return FAILURE; 
            Row--;
            return SUCCESS;
        }
        public int goDown()
        {
            // [column , row]
            if (Neighbors[1, 2] == NeighborStatus.BLOCKED)
                return FAILURE;
            Row++;
            return SUCCESS;
        }
        public int goLeft()
        {
            // [column , row]
            if (Neighbors[0, 1] == NeighborStatus.BLOCKED)
                return FAILURE;
            Column--;
            return SUCCESS;
        }
        public int goRight()
        {
            // [column , row]
            if (Neighbors[2, 1] == NeighborStatus.BLOCKED)
                return FAILURE;
            Column++;
            return SUCCESS;
        }
        #endregion

        /**
         *  Player moves Randomly
         *  @return 0 on sucess, -1 otherwise
         */
        public int makeMove()
        {
            if (toCheck)
            {
                // TODO: Check if piece is a sham one
//                if (Piece.isSham)   // if a piece is a sham
//                    Piece = null;   // destroy it, by simply forgetting 
                toCheck = false;
                return SUCCESS;
            }

            // [column , row]

            #region Go With a Piece
            if (hasPiece)
            {
                if (Team == TeamColor.RED)
                {
                    if (Neighbors[1, 0] != NeighborStatus.BLOCKED &&
                        (Neighbors[1, 1] & NeighborStatus.GOAL_AREA) != NeighborStatus.GOAL_AREA)
                        return goUp();
                    return goForGoalAlternative(TeamColor.RED);

                }
                else
                {
                    if (Neighbors[1, 2] != NeighborStatus.BLOCKED &&
                        (Neighbors[1, 1] & NeighborStatus.GOAL_AREA) != NeighborStatus.GOAL_AREA)
                        return goDown();
                    return goForGoalAlternative(TeamColor.BLUE);
                }
            }
            #endregion

            #region Go Back to the Task Area
            if ((Neighbors[1, 1] & NeighborStatus.GOAL_AREA) == NeighborStatus.GOAL_AREA)
            {
                if (Team == TeamColor.RED && Neighbors[1, 2] != NeighborStatus.BLOCKED)
                {
                    if (goDown() == SUCCESS) return SUCCESS;
                    if (goRight() == SUCCESS) return SUCCESS;
                    if (goLeft() == SUCCESS) return SUCCESS;
                    if (goUp() == SUCCESS) return SUCCESS;
                }
                else if (Neighbors[1, 0] != NeighborStatus.BLOCKED)
                {
                    if (goUp() == SUCCESS) return SUCCESS;
                    if (goRight() == SUCCESS) return SUCCESS;
                    if (goLeft() == SUCCESS) return SUCCESS;
                    if (goDown() == SUCCESS) return SUCCESS;
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
                    case 0: if (goUp()    == SUCCESS) return SUCCESS; else break;
                    case 1: if (goDown()  == SUCCESS) return SUCCESS; else break;
                    case 2: if (goLeft()  == SUCCESS) return SUCCESS; else break;
                    case 3: if (goRight() == SUCCESS) return SUCCESS; else break;
                }
            }
            #endregion
        }

        private int goForGoalAlternative(TeamColor color)
        {

            while (true)
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

                        if (color == TeamColor.RED)
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

    }
}
