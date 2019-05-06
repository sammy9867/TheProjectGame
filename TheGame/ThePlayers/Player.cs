using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePlayers
{
    public class Player
    {
        private const int SUCCESS = 0;
        private const int FAILURE = -1;

        public enum Role { MEMBER, LEADER }
        public enum TeamColor { BLUE, RED };
        public enum NeighborStatus
        {
            FR = 0, // 0000
            PC = 1, // 0001

            GA = 4, // 0100
            NG = 6, // 0110
            DG = 7, // 0111

            BL = 8, // 1000
        }
        public enum BoardCell
        { 
            EC = 0b0_0000_0000,  // Empty Cell       0000 0000
            GC = 0b0_0000_0100,  // Empty Goal Cell  0000 0100
            GL = 0b0_0000_0111,  // Goal             0000 0111
            NG = 0b0_0000_0110,  // NonGoal          0000 0110
            PC = 0b0_0010_0000,  // Piece            0010 0000
            SH = 0b0_0011_0000,  // SHam             0011 0000
            PL = 0b0_0100_0000,  // Player           0100 0000
            ME = 0b0_1000_0000   // ME               1000 0000
        }
        public enum Decision
        {
            MOVE_NORTH, MOVE_SOUTH, MOVE_EAST, MOVE_WEST, DISCOVER, PICKUP_PIECE,
            TEST_PIECE, DESTROY_PIECE, PLACE_PIECE, KNOWLEDGE_EXCHANGE
        }

        public NeighborStatus[,] Neighbors { get; set; }  // [row, col]

        public BoardCell[,] Board { get; set; } // [row, column]
        public BoardCell    current;
        #region Board Dimensions
        public int BoardWidth { get; set; }
        public int BoardTaskHeight { get; set; }
        public int BoardGoalHeight { get; set; }
        public int BoardHeight { get { return BoardTaskHeight + 2*BoardGoalHeight; } }
        #endregion 

        public string ID { get; set; }
        public Role role { get; set; }
        public bool hasPiece { get; set; }
        public bool isSham { get; set; } //CREATED A VARIABLE FOR SHAM FOR NOW.

        public int TeamSize { get; set; }
        public List<string> Mates {get;set;}

        #region coordinates
        public int Row { get; set; }
        public int Column { get; set; }
        public int X { get { return Column; } set { Column = X; } }
        public int Y { get { return Row; } set { Row = Y; } }
        #endregion

        public TeamColor Team { get; set; }

        private enum AlternativeStep { NORTH, SOUTH, WEST, EAST }

        public bool SendDiscover = false;
        public string ApplyiedDirection;

        #region MOVES
        public int TryMoveNorth()
        {
            // [row , col]
            if (Neighbors[0, 1] == NeighborStatus.BL)
                return FAILURE; 
//            Row--;
            return SUCCESS;
        }
        public int TryMoveSouth()
        {
            // [row , col]
            if (Neighbors[2, 1] == NeighborStatus.BL)
                return FAILURE;
//            Row++;
            return SUCCESS;
        }
        public int TryMoveWest()
        {
            // [row , col]
            if (Neighbors[1, 0] == NeighborStatus.BL)
                return FAILURE;
//            Column--;
            return SUCCESS;
        }
        public int TryMoveEast()
        {
            // [row col]
            if (Neighbors[1, 2] == NeighborStatus.BL)
                return FAILURE;
//            Column++;
            return SUCCESS;
        }
        #endregion

        /**
         *  Player moves Randomly
         *  @return 0 on sucess, -1 otherwise
         */
        public Decision MakeMove()
        {
            if (SendDiscover == true)
            {
                SendDiscover = false;
                return Decision.DISCOVER;
            }

            if (current == BoardCell.PC)
                return Decision.PICKUP_PIECE;

            if (hasPiece && !isSham)
                return Decision.TEST_PIECE; // always test piece once you have it

            if (hasPiece && isSham)
                return Decision.DESTROY_PIECE;

            // Nowhere to go 
            if (Neighbors[1, 0] == NeighborStatus.BL && Neighbors[1, 2] == NeighborStatus.BL &&
                Neighbors[0, 1] == NeighborStatus.BL && Neighbors[2, 1] == NeighborStatus.BL)
                return Decision.DISCOVER;

            // row, col
            #region Go With a Piece
            if (hasPiece)
            {
                if (Team == TeamColor.RED)
                {
                    if (TryMoveNorth() == SUCCESS)
                        return Decision.MOVE_NORTH;
                    return goForGoalAlternative(TeamColor.RED);

                }
                else
                {
                    if (TryMoveSouth() == SUCCESS)
                        return Decision.MOVE_SOUTH;
                    return goForGoalAlternative(TeamColor.BLUE);
                }
            }
            #endregion

            #region Go Back to the Task Area No Piece
            if ((Neighbors[1, 1] & NeighborStatus.GA) == NeighborStatus.GA)
            {
                if (Team == TeamColor.RED)
                {
                    if (TryMoveSouth() == SUCCESS) return Decision.MOVE_SOUTH;
                    if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST;
                    if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST;
                    if (TryMoveNorth() == SUCCESS) return Decision.MOVE_NORTH;
                    return Decision.DISCOVER;
                }
                else 
                {
                    if (TryMoveNorth() == SUCCESS) return Decision.MOVE_NORTH;
                    if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST;
                    if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST;
                    if (TryMoveSouth() == SUCCESS) return Decision.MOVE_SOUTH;
                    return Decision.DISCOVER;
                }
            }
            #endregion

            // TODO: Manhattan Distance  !!!!
            #region To Neighbouring piece  ROW COLUMN
            if (Neighbors[0, 1] == NeighborStatus.PC) return Decision.MOVE_NORTH;
            if (Neighbors[1, 0] == NeighborStatus.PC) return Decision.MOVE_WEST;
            if (Neighbors[2, 1] == NeighborStatus.PC) return Decision.MOVE_SOUTH;
            if (Neighbors[1, 2] == NeighborStatus.PC) return Decision.MOVE_EAST;

            if (Neighbors[0, 0] == NeighborStatus.PC)
            {
                if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST; else return Decision.MOVE_NORTH;
            }
            if (Neighbors[2, 0] == NeighborStatus.PC)
            {
                if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST; else return Decision.MOVE_SOUTH;
            }
            if (Neighbors[2, 2] == NeighborStatus.PC)
            {
                if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST; else return Decision.MOVE_SOUTH;
            }
            if (Neighbors[0, 2] == NeighborStatus.PC)
            {
                if (TryMoveNorth() == SUCCESS) return Decision.MOVE_NORTH; else return Decision.MOVE_EAST;
            } 
            #endregion

            #region Go Random
            /* Go Random */
            Random r = new Random(Guid.NewGuid().GetHashCode());
            while (true)
            {
                switch (r.Next() % 4)
                {
                    case 0: if (TryMoveNorth()    == SUCCESS) return Decision.MOVE_NORTH; else break;
                    case 1: if (TryMoveSouth()  == SUCCESS) return Decision.MOVE_SOUTH; else break;
                    case 2: if (TryMoveWest()  == SUCCESS) return Decision.MOVE_WEST; else break;
                    case 3: if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST; else break;
                }
            }
            #endregion
        }

        private Decision goForGoalAlternative(TeamColor color)
        {
            AlternativeStep GoalStep = AlternativeStep.WEST;
            int counter = 4;
            while (true)
            {
                counter--;
                switch (GoalStep)
                {
                    case AlternativeStep.WEST:
                        if (TryMoveWest() == SUCCESS)
                            return Decision.MOVE_WEST;
                        GoalStep = AlternativeStep.EAST;
                        break;

                    case AlternativeStep.EAST:
                        if (TryMoveEast() == SUCCESS)
                            return Decision.MOVE_EAST;

                        if (color == TeamColor.RED)
                            GoalStep = AlternativeStep.NORTH;
                        else
                            GoalStep = AlternativeStep.SOUTH;
                        break;

                    case AlternativeStep.NORTH:
                        if (TryMoveNorth() == SUCCESS)
                            return Decision.MOVE_NORTH;
                        GoalStep = AlternativeStep.SOUTH;
                        break;

                    case AlternativeStep.SOUTH:
                        if (TryMoveSouth() == SUCCESS)
                            return Decision.MOVE_SOUTH;

                        GoalStep = AlternativeStep.NORTH;
                        break;
                }
                if (counter == 0)
                    return Decision.DISCOVER;
            }
        }


        //* Execute Movement on Successful responce */
        public void DoMove(string direction)
        {
            switch (direction.ToUpper())
            {
                case "N": MoveNorth(); break;
                case "S": MoveSouth(); break;
                case "E": MoveEast(); break;
                case "W": MoveWest(); break;
            }
        }
        private void MoveNorth() => Row--;
        private void MoveSouth() => Row++;
        private void MoveWest()  => Column--;
        private void MoveEast()  => Column++;

    }
}
