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
        public enum NeighborStatus { PIECE, GOAL_AREA, FREE, BLOCKED }

        public NeighborStatus[,] Neighbors { get; set; }  // [column, row]
        public int playerID { get; set; }
        public int row { get; set; }
        public int column { get; set; }
        public Role role { get; set; }
        public Piece Piece { get; set; }
        public Team.TeamColor Team { get; set; }


        private bool toCheck;

        #region MOVES
        /**
         *  Player moves Up
         *  @return 0 on sucess, -1 otherwise
         */
        public int goUp()
        {
            if (Team == Model.Team.TeamColor.RED && row == 0)
                return -1;
            if (Team == Model.Team.TeamColor.BLUE && row == Board.GoalHeight)
                return -1;

            row--;
            return 0;
        }

        /**
         *  Player moves Down
         *  @return 0 on sucess, -1 otherwise
         */
        public int goDown()
        {
            if (Team == Model.Team.TeamColor.BLUE && row == Board.Height - 1)
                return -1;
            if (Team == Model.Team.TeamColor.RED && row == Board.Height - Board.GoalHeight - 1)
                return -1;

            row++;
            return 0;
        }
            
        /**
         *  Player moves Left
         *  @return 0 on sucess, -1 otherwise
         */
        public int goLeft()
        {
            if (column == 0)
                return -1;
            column--;
            return 0;
        }

        /**
         *  Player moves Right
         *  @return 0 on sucess, -1 otherwise
         */
        public int goRight()
        {
            if (Board.Width - 1 == column)
                return -1;
            column++;
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

            /* Has a piece ? */
            if (null != Piece)
            {
                if (Team == Model.Team.TeamColor.RED)
                {
                    int m = goUp();
                    if (m != 0)
                    {
                        Piece = null;
                        return 3;
                    }
                    return m;
                }
                else
                {
                    int m = goDown();
                    if (m == -1)
                    {
                        Piece = null;
                        return 3;
                    }
                    return m;
                }
            }
            else if (Neighbors[1, 1] == NeighborStatus.GOAL_AREA)
            {
                if (Team == Model.Team.TeamColor.RED && Neighbors[1,2] != NeighborStatus.BLOCKED)
                {
                    return goDown();
                }
                else if (Neighbors[1,0] != NeighborStatus.BLOCKED)
                {
                    return goUp();
                }
            }

            // [column , row]
            
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
