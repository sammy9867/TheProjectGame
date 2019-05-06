using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThePlayers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace UnitTestProject_ForExceptions
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Something went wrong :(")]
        public void TestMethod_PlayerSocket_Receive()
        {
            //Action<string> st = null;
            //ThePlayers.PlayerSocket.Receive(st);

        }





        [TestMethod]
        public void TestMethod_PlayerSocket_StartClient()
        {
            Player player = new Player
            {
                ID = "" + (new Random()).Next(),
                Team = Player.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3],
                Piece = null
            };

            // Initialize player 
            PlayerSocket.Player = player;
            //CommunicationServer.Server.Main(null);
            //System.Threading.Thread.Sleep(1000);
            // Start Communication with CS
            PlayerSocket.StartClient();


        }

    }




}
