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

        public string playerID { get; set; }

        public int Row { get; set; }
        public int Column { get; set; }
        public int X { get { return Column; } set { Column = X; } }
        public int Y { get { return Row; }  set { Row = Y; } }

        public Role role { get; set; }
        public Piece Piece { get; set; }

        public Team.TeamColor Team { get; set; }

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

   
        public void checkPiece()
        {
            if (Piece == null) return;
        }

        public bool hasPiece()
        {
            if (null == Piece)
                return false;
            return true;
        }
    }
}
