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
        public int playerID { get; set; }
        public int row { get; set; }
        public int column { get; set; }
        public Role role { get; set; }
        public bool hasPiece { get; set; }
        public Team.TeamColor Team { get; set; }

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


        /**
         *  Player moves Randomly
         *  @return 0 on sucess, -1 otherwise
         */
        public int goRnd()
        {
            if (Team == Model.Team.TeamColor.RED && hasPiece)
            {
                int m = goUp();
                if (m != 0)
                {
                    hasPiece = false;
                    return 7;
                }
                return m;
            }

            if (Team == Model.Team.TeamColor.BLUE && hasPiece)
            {
                int m = goDown();
                if (m == -1)
                {
                    hasPiece = false;
                    return 7;
                }
                return m;
            }


            Random r = new Random(Guid.NewGuid().GetHashCode());
            switch (r.Next() % 4)
            {
                case 0: return goUp();
                case 1: return goDown();
                case 2: return goLeft();
                case 3: return goRight();
            }
            return -1;
        }
    }
}
