using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThePlayers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
//using Newtonsoft.Json;
namespace UnitTestProject_ForExceptions
{
    [TestClass]
    public class UnitTest1
    {
      

        [TestMethod]
        public void TestMethod_PlayerSocket_StartClient()
        {
            Player player = new Player
            {
          //      ID = "" + (new Random()).Next(),
                Team = Player.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3],
                Piece = null
            };

            // Initialize player 
            PlayerSocket.Player = player;
            Process.Start(@"C:\Users\M.Abouelsaadat\Desktop\SEProject\theprojectgame\TheGame\CommunicationServer\bin\Debug\CommunicationServer");
            // Start Communication with CS
       //     PlayerSocket.StartClient();
        }




        [TestMethod]
        public void TestMethod_PlayerSocket_Send()
        {
            Player player = new Player
            {
             //   ID = "" + (new Random()).Next(),
                Team = Player.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3],
                Piece = null
            };

            // Initialize player 
            PlayerSocket.Player = player;
            Process.Start(@"C:\Users\M.Abouelsaadat\Desktop\SEProject\theprojectgame\TheGame\CommunicationServer\bin\Debug\CommunicationServer");
            // Start Communication with CS
     //       PlayerSocket.StartClient();
       //     PlayerSocket.Send(PlayerSocket.socket, JsonConvert.SerializeObject("start"));
        }


    }




}
