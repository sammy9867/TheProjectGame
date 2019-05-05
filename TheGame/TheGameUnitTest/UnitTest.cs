using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheGame.Model;
using System.Collections.Generic;
using TheGame.GMServer;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text;
//using Microsoft.CSharp.RuntimeBinder.CSharp.ArgumentInfo.Create;

namespace TheGameUnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod_ForGMRequestHandler()
        {
            /* for SendSetUpGame */
            string file = @"C:\Users\julia\source\repos\theprojectgame\TheGame\TheGame\JSONs\SetUpGame.json";

            string expected = File.ReadAllText(file, Encoding.ASCII); ;
            string actual = GMRequestHandler.SendSetUpGame();

            Assert.AreNotEqual(expected, actual);

            /*for ConnectPlayerOK */
            Player pl = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 0,
                Row = 0,
                Column = 0,
                //toCheck = true,
                Team = Team.TeamColor.BLUE
            };
            //string file0 = @"C:\Users\julia\source\repos\theprojectgame\TheGame\TheGame\JSONs\ConfirmJoiningGame.json";
            //string json = File.ReadAllText(file0, Encoding.ASCII); ;
            //dynamic expected0 = JsonConvert.DeserializeObject(json);
            //expected0.userGuid = pl.playerID;

            //dynamic actual0 = GMRequestHandler.ConnectPlayerOK(pl);
            //Assert.AreEqual(expected0, actual0);

            /* for ConnectPlayerDeny */


            /* for BeginGame */
            Player p1 = new Player()
            {
                role = Player.Role.LEADER,
                //playerID = 0,
                Row = 0,
                Column = 0,
                //toCheck = true,
                Team = Team.TeamColor.RED
            };

            Player p2 = new Player()
            {
                role = Player.Role.MEMBER,
                //playerID = 1,
                Row = 1,
                Column = 10,
                //toCheck = true,
                Team = Team.TeamColor.RED
            };

            List<Player> listp = new List<Player>()
            {
                p1,p2
            };

            //string res = GMRequestHandler.BeginGame(p2, listp, p1);
            

            /* for ResponseForDiscover */
        }

        /* TODO */
        //[TestMethod]
        //public void TestMethod_ForServer()
        //{
        //    string[] args = { "fdg", "dfdskjb" };
        //    int expected = 0;
        //    int actual = CommunicationServer.Server.Main(args);

        //    Assert.AreEqual(expected, actual);
        //}
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
            int ex = 2;

            int actual0 = t.NumOfPlayers;           
            Team.TeamCell actual = t.isTaken(col, row);
            Team.TeamCell actual2 = t.isDiscovered(col, row);
            Assert.AreEqual(ex, actual0);
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
            int expected = 17;

            Assert.AreEqual(expected, actual);

            bool actual2 = b.IsUndiscoveredGoal(col, row);
            bool expected2 = false;

            Assert.AreEqual(expected2, actual2);

            Team.TeamColor col_blue = Team.TeamColor.BLUE;
            Player.NeighborStatus actual3 = b.GetPlayersNeighbor(col, row, col_blue);
            Player.NeighborStatus expected3 = Player.NeighborStatus.BLOCKED;

            Assert.AreEqual(expected3, actual3);

            int col1 = 1, row1 = 1;
            Team.TeamColor col_red = Team.TeamColor.RED;
            Player.NeighborStatus actual4 = b.GetPlayersNeighbor(col1, row1, col_red);
            Player.NeighborStatus expected4 = Player.NeighborStatus.BLOCKED;
            Assert.AreEqual(expected4, actual4);
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
        public void TestMethod_ForPlayerInThePlayers()/* COMPLETE */
        {
            ThePlayers.Player pl = new ThePlayers.Player()
            {
                role = ThePlayers.Player.Role.LEADER,
                hasPiece = false,
                
            };
        }
        [TestMethod]
        public void TestMethod_ForPlayer()/* COMPLETE */
        {
            Player p = new Player();
            //p.Neighbors = new Player.NeighborStatus[2, 2];

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
            //Assert.AreEqual(p.goRnd(), -1);

            //Assert.AreEqual(p.goForGoalAlternative(p.Team), 0);
            Assert.IsTrue(p.hasPiece());
        }
    }

}
