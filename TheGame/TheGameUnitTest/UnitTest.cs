using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheGame.Model;
using System.Collections.Generic;
namespace TheGameUnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod_ForPiece() /* COMPLETE */
        {
            //Arrange

            int column = 1;
            int row = 2;
            Piece p = new Piece();
            p.column = 1;
            p.row = 2;
            bool expected = true;

            //Act

            bool actual = p.isTaken(column, row);

            //Assert
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_ForTeam() /* COMPLETE */
        {
            Player p_blue = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 0,
                Row = 0,
                Column = 0,
                //toCheck = true,
                Team = Team.TeamColor.BLUE
            };

            Player p_red = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 1,
                Row = 1,
                Column = 10,
                //toCheck = true,
                Team = Team.TeamColor.RED
            };

            List<Player> listp = new List<Player>()
            {
                p_blue,p_red
            };
            Goal g1 = new Goal()
            {
                column = 0,
                row = 1,
                isGoal = true
            };

            Goal g2 = new Goal()
            {
                column = 1,
                row = 2,
                isGoal = true
            };

            List<Goal> listg = new List<Goal>()
            {
                g1,g2
            };

            Team t = new Team();
            t.members = listp;
            t.DiscoveredGoals = listg;
            t.DiscoveredNonGoals = listg;
            Player p = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 1,
                Row = 0,
                Column = 1,
                //toCheck = true,
                Team = Team.TeamColor.RED
            };

            t.leader = p;
            int col = 1;
            int row = 0;
            Team.TeamCell expected = Team.TeamCell.FREE;

            Team.TeamCell actual = t.isTaken(col, row);
            Team.TeamCell actual2 = t.isDiscovered(col, row);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);

        }

        [TestMethod]
        public void TestMethod_ForBoard() /* COMPLETE */
        {
            Piece p1 = new Piece()
            {
                pieceID = 0,
                isSham = false,
                row = 0,
                column = 0
            };

            Piece p2 = new Piece()
            {
                pieceID = 1,
                isSham = false,
                row = 1,
                column = 1
            };

            List<Piece> listp = new List<Piece>()
            {
                p1,p2
            };
            Board b = new Board();
            b.Pieces = listp;

            Player p_blue = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 0,
                Row = 0,
                Column = 0,
                //toCheck = true,
                Team = Team.TeamColor.BLUE
            };

            Player p_red = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 1,
                Row = 1,
                Column = 10,
                //toCheck = true,
                Team = Team.TeamColor.RED
            };
            Goal g1 = new Goal()
            {
                column = 0,
                row = 1,
                isGoal = true
            };

            Goal g2 = new Goal()
            {
                column = 1,
                row = 2,
                isGoal = true
            };

            List<Goal> listg = new List<Goal>()
            {
                g1,g2
            };

            b.DiscoveredBlueGoals = listg;
            b.DiscoveredRedGoals = listg;
            b.UndiscoveredBlueGoals = listg;
            b.UndiscoveredRedGoals = listg;
            Team t_blue = new Team()
            {
                leader = p_blue
            };
            b.BlueTeam = t_blue;

            Team t_red = new Team()
            {
                leader = p_red
            };
            b.BlueTeam = t_red;

            Board.GoalHeight = 20;
            Board.Height = 10;

            int col = 0, row = 0;

            int actual = b.getCellStatus(col, row);
            int expected = 12;

            Assert.AreEqual(expected, actual);
            //Assert.IsFalse(b.IsUndiscoveredGoal(col, row));
        }

        [TestMethod]
        public void TestMethod_ForGoal() /* COMPLETE */
        {
            int column = 1;
            int row = 0;
            Goal g = new Goal();
            g.column = 0;
            g.row = 0;
            bool expected = false;

            bool actual = g.isTaken(column, row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMethod_ForPlayer()/* COMPLETE */
        {
            Player p = new Player();
            p.Neighbors = new Player.NeighborStatus[2, 2];

            Piece pi = new Piece()
            {
                pieceID = 0,
                isSham = false,
                row = 0,
                column = 0
            };
            p.Piece = pi;
            p.Row = 0;
            Assert.AreEqual(p.goUp(), -1);

            p.Column = 1;
            p.Team = Team.TeamColor.RED;
            Assert.AreEqual(p.goLeft(), 0);

            Board.Width = 2;
            Assert.AreEqual(p.goRight(), 0);

            Board.Height = 3;
            Board.GoalHeight = 2;
            Assert.AreEqual(p.goDown(), -1);

            //p.toCheck = true;
            Assert.AreEqual(p.goRnd(), -1);

            //Assert.AreEqual(p.goForGoalAlternative(p.Team), 0);
            Assert.IsTrue(p.hasPiece());
        }
    }

}
