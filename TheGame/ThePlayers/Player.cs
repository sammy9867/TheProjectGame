using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ThePlayers
{
    public class Player
    {
        private const int SUCCESS = 0;
        private const int FAILURE = -1;

        public Piece Piece;

        public enum Role { MEMBER, LEADER }
        public enum TeamColor { BLUE, RED };
        public enum NeighborStatus
        {
            FR = 0, // 0000
            PC = 1, // 0001

            GA = 4, // 0100
            DG = 6, // 0110
            
            BL = 8, // 1000
        }
        public enum BoardCell
        { 
            EC = 0b0_0000_0000,  // Empty Cell       0000 0000
            GC = 0b0_0000_0010,  // Goal Cell        0000 0010
            GL = 0b0_0000_0011,  // Goal             0000 0011
            PC = 0b0_0010_0000,  // Piece            0010 0000
            SH = 0b0_0011_0000,  // SHam             0011 0000
            PL = 0b0_0100_0000,  // Player           0100 0000
            ME = 0b0_1100_0000   // ME               1100 0000
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

        public string private_id { set; get; }
        public string ID { get { return ComputeSha256Hash(); } }
        public Role role { get; set; }
        public bool hasPiece
        {
            get { if (Piece == null) return false; else return true; }
        }

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

        public bool KnowledgeExchange = false;
        public bool SendDiscover = false;
        public string ApplyiedDirection;

        #region MOVES
        public int TryMoveNorth()
        {
            // [row , col]
            if (Neighbors[2, 1] == NeighborStatus.BL)  //BEFORE [0, 1]
                return FAILURE; 
            if(hasPiece && (Neighbors[2, 1] == NeighborStatus.DG || Neighbors[2, 1] == NeighborStatus.DG))
                return FAILURE;
            //            Row--;
            return SUCCESS;
        }
        public int TryMoveSouth()
        {
            // [row , col] 
            if (Neighbors[0, 1] == NeighborStatus.BL)   //BEFORE [2, 1]
                return FAILURE;

            if (hasPiece && (Neighbors[0, 1] == NeighborStatus.DG || Neighbors[0, 1] == NeighborStatus.DG))
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
            /* Knowledge Exchange */
            if (KnowledgeExchange == true)
            {
                KnowledgeExchange = false;
                return Decision.KNOWLEDGE_EXCHANGE;
            }

            /* Forced Discover */
            if (SendDiscover == true)
            {
                SendDiscover = false;
                return Decision.DISCOVER;
            }

            /* Pickup a piece */
            if (!hasPiece && current == BoardCell.PC)
            {
                // Whenever we pickup unchecked piece we assume it is not a sham
                Board[Y, X] = current = BoardCell.EC;
                return Decision.PICKUP_PIECE;
            }

            /* Player has a piece that never was tested */
            if (hasPiece && !Piece.isTested)
                return Decision.TEST_PIECE; // always test piece once you have it

            /* Player has a sham */
            if (hasPiece && Piece.isSham)
                return Decision.DESTROY_PIECE;

            /* if a Player has a piece => the pieece is tested and it is not sham */

            // Nowhere to go 
            if (Neighbors[1, 0] == NeighborStatus.BL && Neighbors[1, 2] == NeighborStatus.BL &&
                Neighbors[0, 1] == NeighborStatus.BL && Neighbors[2, 1] == NeighborStatus.BL)
                return Decision.DISCOVER;


            // row, col
            #region Go With a Piece
            if (hasPiece)
            {
                return GoWithPiece();
            }
            #endregion

            #region Go Back to the Task Area No Piece
//            if ((Neighbors[1, 1] & NeighborStatus.GA) == NeighborStatus.GA)
            if((current & BoardCell.GC) == BoardCell.GC)
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
            if (Neighbors[0, 1] == NeighborStatus.PC) return Decision.MOVE_SOUTH;
            if (Neighbors[1, 0] == NeighborStatus.PC) return Decision.MOVE_WEST;
            if (Neighbors[2, 1] == NeighborStatus.PC) return Decision.MOVE_NORTH;
            if (Neighbors[1, 2] == NeighborStatus.PC) return Decision.MOVE_EAST;

            if (Neighbors[0, 0] == NeighborStatus.PC)
            {
                if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST;
                if (TryMoveSouth() == SUCCESS) return Decision.MOVE_SOUTH;
                return Decision.DISCOVER;
            }
            if (Neighbors[2, 0] == NeighborStatus.PC)
            {
                if (TryMoveWest() == SUCCESS) return Decision.MOVE_WEST;
                if (TryMoveNorth() == SUCCESS) return Decision.MOVE_NORTH;
                return Decision.DISCOVER;
            }
            if (Neighbors[2, 2] == NeighborStatus.PC)
            {
                if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST;
                if (TryMoveNorth() == SUCCESS) return Decision.MOVE_NORTH;
                return Decision.DISCOVER;
            }
            if (Neighbors[0, 2] == NeighborStatus.PC)
            {
                if (TryMoveEast() == SUCCESS) return Decision.MOVE_EAST;
                if (TryMoveSouth() == SUCCESS) return Decision.MOVE_SOUTH;
                return Decision.DISCOVER;
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

        private Decision GoWithPiece()
        {
//            if (current == BoardCell.GC) return Decision.PLACE_PIECE;

            if (Team == TeamColor.RED)
            {
                if (TryMoveNorth() == SUCCESS)
                    return Decision.MOVE_NORTH;

                //Once reached goal area
                if (current == BoardCell.GC) return Decision.PLACE_PIECE;
                return goForGoalAlternative(TeamColor.RED);

            }
            else
            {
                if (TryMoveSouth() == SUCCESS)
                    return Decision.MOVE_SOUTH;
                if (current == BoardCell.GC) return Decision.PLACE_PIECE;
                return goForGoalAlternative(TeamColor.BLUE);
            }

        }

        AlternativeStep AlternativeGoalStep = AlternativeStep.WEST;
        //was private
        public Decision goForGoalAlternative(TeamColor color)
        {
            int counter = 4;
            while (true)
            {
                counter--;

                //Same Logic with BLUE PLAYER [HERE, MOVES NORTH WHEN REACHED EXTREME EAST]
                // Player that placed a piece shall notify team mates about teste goal cell 
                switch (AlternativeGoalStep)
                {
                    case AlternativeStep.WEST:
                        if (TryMoveWest() == SUCCESS)
                            return Decision.MOVE_WEST;
                        AlternativeGoalStep = AlternativeStep.EAST;  
                        break;

                    case AlternativeStep.EAST:
                        if (TryMoveEast() == SUCCESS)
                            return Decision.MOVE_EAST;

                        if (color == TeamColor.RED)
                        {
                            if (TryMoveSouth() == SUCCESS)
                                return Decision.MOVE_SOUTH;
                            AlternativeGoalStep = AlternativeStep.WEST;
                        }
                        else
                        {
                            if (TryMoveNorth() == SUCCESS)
                                return Decision.MOVE_NORTH;
                            AlternativeGoalStep = AlternativeStep.WEST;
                        }
                        break;

                    case AlternativeStep.NORTH:
                        if (TryMoveNorth() == SUCCESS)
                            return Decision.MOVE_NORTH;
                        AlternativeGoalStep = AlternativeStep.WEST;
                        break;

                    case AlternativeStep.SOUTH:
                        if (TryMoveSouth() == SUCCESS)
                            return Decision.MOVE_SOUTH;

                        AlternativeGoalStep = AlternativeStep.WEST;
                        break;
                }
                if (counter == 0)
                    return Decision.DISCOVER;
            }
        }


        //* Execute Movement on Successful responce */
        public void DoMove(string direction)
        {
            // TODO: here is temporary solution on double responce
            if (direction == null)
            {
                Console.WriteLine("\tERROR\ndirection = null");
                return;
            }
            switch (direction.ToUpper())
            {
                case "N": MoveNorth(); break;
                case "S": MoveSouth(); break;
                case "E": MoveEast(); break;
                case "W": MoveWest(); break;
            }
        }
        public void MoveNorth() => Row++;
        public void MoveSouth() => Row--;
        public void MoveWest()  => Column--;
        public void MoveEast()  => Column++;


        private string ComputeSha256Hash()
        {
            return private_id;
            string rawData = private_id;
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }


    public class Piece
    {
        public bool isTested;
        public bool isSham;

        public Piece()
        { isTested = false; isSham = false; }
    }
}
