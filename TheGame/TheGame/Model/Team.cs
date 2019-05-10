using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class Team
    {
        public enum TeamColor { BLUE, RED };
        public enum TeamCell { FREE, PLAYER, PLAYER_PIECE, DISCOVERED_GOAL, DISCOVERED_NONGOAL};

        public Player leader;
        public List<Player> members;

        /* Temporary Lists, later every player will have its own lists */
        public List<Goal> DiscoveredGoals;
        public List<Goal> DiscoveredNonGoals;

        public TeamColor teamColor { get; set; }

        public int NumOfPlayers
        { get
            {
                return members == null ? -1 : members.Count();
            }
        }

        public Team(TeamColor teamColor)
        {
            this.members = new List<Player>();
            this.DiscoveredGoals = new List<Goal>();
            this.DiscoveredNonGoals = new List<Goal>();
            this.teamColor = teamColor;
        }


        public TeamCell isTaken(int col, int row)
        {
            //if (leader.column == col && leader.row == row)
            //{
            //    if (leader.hasPiece)
            //        return 2;
            //    else return 1;
            //}

            foreach (var item in members)
            {
                if (item.Row == row && item.Column == col)
                {
                    if (item.hasPiece()) return TeamCell.PLAYER_PIECE;
                    else return TeamCell.PLAYER;
                }
            }

            foreach (var goal in DiscoveredGoals)
                if (goal.isTaken(col, row))
                    return TeamCell.DISCOVERED_GOAL;

            foreach (var goal in DiscoveredNonGoals)
                if (goal.isTaken(col, row))
                    return TeamCell.DISCOVERED_NONGOAL;

            return TeamCell.FREE;
        }

        public TeamCell isDiscovered(int col, int row)
        {
            foreach (var goal in DiscoveredGoals)
                if (goal.isTaken(col, row))
                    return TeamCell.DISCOVERED_GOAL;

            foreach (var goal in DiscoveredNonGoals)
                if (goal.isTaken(col, row))
                    return TeamCell.DISCOVERED_NONGOAL;

            return TeamCell.FREE;
        }
    }

}
