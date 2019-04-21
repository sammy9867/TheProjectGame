using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace CommunicationServer
{
    public static class CSRequestHandler
    {

        public static bool beginGame { get; set; }

        //Sending ConfirmSetUpGame JSON to to client on connection
        public static void SendConfirmGame(Socket handler)
        {
            string file = @"..\..\JSONs\ConfirmSetUpGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string action = magic.action;
            magic.result = "ok";        // TODO:

            Server.Send(handler, JsonConvert.SerializeObject(magic));

        }

        public static void ConnectPlayer(String data, Socket gMSocket)
        {
            Console.WriteLine("ConnectPlayer");
            Server.Send(gMSocket, data);
        }

        internal static void ConnectPlayerConfirmation(String data, Socket destPlayer)
        {
            Console.WriteLine("ConnectPlayer");
            Server.Send(destPlayer, data);
        }

        internal static void BeginPlayer(string data, Socket destPlayer)
        {
            Console.WriteLine("BeginPlayer");
            Server.Send(destPlayer, data);
            beginGame = true; 
        }

        #region Communication Phase2

        //Move action is sent by playerto GM, so the destination socket will be GM socket.
        internal static void Move(string data, Socket gmSocket)
        {
            Console.WriteLine("Move");
            Server.Send(gmSocket, data);
        }

        //Response for Move action is sent by GM to player, so the destination socket will be destPLayer.
        internal static void ResponseForValidMove(string data, Socket destPlayer)
        {
            Console.WriteLine("Response for Valid Move");
            Server.Send(destPlayer, data);
        }

        internal static void ResponseForInvalidValidMove(string data, Socket destPlayer)
        {
            Console.WriteLine("Response for Invalid Move");
            Server.Send(destPlayer, data);
        }


        internal static void Discover(string data, Socket gmSocket)
        {
            Console.WriteLine("Discover");
            Server.Send(gmSocket, data);
        }

        internal static void ResponseForDiscover(string data, Socket destPlayer)
        {
            Console.WriteLine("Response for Discover");
            Server.Send(destPlayer, data);
        }

        internal static void PickUpPiece(string data, Socket gmSocket)
        {
            Console.WriteLine("PickUpPiece");
            Server.Send(gmSocket, data);
        }

        internal static void TestPiece(string data, Socket gmSocket)
        {
            Console.WriteLine("TestPiece");
            Server.Send(gmSocket, data);
        }

        internal static void DestroyPiece(string data, Socket gmSocket)
        {
            Console.WriteLine("DestroyPiece");
            Server.Send(gmSocket, data);
        }

        internal static void PlacePiece(string data, Socket gmSocket)
        {
            Console.WriteLine("PlacePiece");
            Server.Send(gmSocket, data);
        }

        #endregion






    }
}
