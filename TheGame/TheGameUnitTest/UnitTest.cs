using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheGame.Model;

namespace TheGameUnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod_ForPiece()
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
        public void TestMethod_ForTeam()
        {
            Team t = new Team();
            t.leader.column = 0;
            t.leader.row = 1;

            int col = 1;
            int row = 0;
            bool expected = false;
            //foreach (var item in t.members)
            //{
                bool actual = t.isTaken(col, row);
                Assert.AreEqual(expected, actual);
            //}
        }

        [TestMethod]
        public void TestMethod_ForBoard()
        {
            Board b = new Board();
            b.BlueTeam.leader.column = 0;
            b.BlueTeam.leader.row = 0;

            b.RedTeam.leader.column = 10;
            b.RedTeam.leader.row = 1;
            Board.GoalHeight = 20;
            Board.Height = 10;

            int col = 0, row = 0;
            bool isTaken_blue = b.BlueTeam.isTaken(col, row); //true
            bool isTaken_red = b.RedTeam.isTaken(col, row); //false

            int expected = (int)Board.Status.TASK_AREA;
            int actual = (int)Board.Status.BLUE_PLAYER;

            Assert.AreEqual(expected, actual);
        }
    }

}
