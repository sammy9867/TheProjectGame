﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ThePlayers
{
    public static class PlayerRequestHandler
    {
        //Sending ConfirmSetUpGame JSON to to client on connection
        public static void sendJoinGame(Socket handler)
        {
            string file = @"..\..\JSONs\JoinGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">"+file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;
            magic.preferredTeam = PlayerSocket.Player.Team == Player.TeamColor.RED ? "red" : "blue";

            Console.WriteLine("Sending JoinGame.json to ComServer.....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));

        }

        public static void sendMove(Socket handler, string direction)
        {
            string file = @"..\..\JSONs\Move.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;

            magic.direction = direction;

            Console.WriteLine("Sending Move.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));

        }

        //2nd Communication phase
        public static void sendDiscover(Socket handler)
        {
            string file = @"..\..\JSONs\Discover.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;

            magic.location.x = PlayerSocket.Player.X;
            magic.location.y = PlayerSocket.Player.Y;

            Console.WriteLine("Sending Discover.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }

        public static void sendPickup(Socket handler)
        {
            string file = @"..\..\JSONs\Pickup.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;
          
            Console.WriteLine("Sending Pickup.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }

        public static void sendTestPiece(Socket handler)
        {
            string file = @"..\..\JSONs\TestPiece.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;

            Console.WriteLine("Sending TestPiece.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }

        public static void sendPlacePiece(Socket handler)
        {
            string file = @"..\..\JSONs\PlacePiece.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;

            Console.WriteLine("Sending TestPiece.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }

        public static void sendDestroyPiece(Socket handler)
        {
            string file = @"..\..\JSONs\DestroyPiece.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = PlayerSocket.Player.ID;

            Console.WriteLine("Sending Destroy.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }


        
        public static void sendAuthorizeKnowledgeExchange(Socket handler, string player, string receiver)
        {
            string file = @"..\..\JSONs\AuthorizeKnowledgeExchange.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player;
            magic.receiverGuid = receiver;

            Console.WriteLine("Sending AuthorizeKnowledgeExchange.json....\n");
            PlayerSocket.Broadcast(handler,
                JsonConvert.SerializeObject(magic));

        }
        public static void sendRejectKnowledgeExchange(Socket handler, string player) {
            string file = @"..\..\JSONs\RejectKnowledgeExchange.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player;
            // "rejectDuration":  "<single|permanent>"
            Console.WriteLine("Sending RejectKnowledgeExchange.json....\n");
            PlayerSocket.Broadcast(handler, // rejecting knowledge exchange - no responce expected
                JsonConvert.SerializeObject(magic));

        }
        public static void sendAcceptKnowledgeExchange(Socket handler, string player) {
            string file = @"..\..\JSONs\AcceptKnowledgeExchange.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player;

            Console.WriteLine("Sending sendAcceptKnowledgeExchange.json....\n");
            PlayerSocket.Send(handler, // expecting the data with knowledge 
                JsonConvert.SerializeObject(magic));
        }
        public static string stringKnowledgeExchangeSend(Socket handler, Player player, string receiver) {
            string file = @"..\..\JSONs\KnowledgeExchangeSend.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: ");
                Console.WriteLine(">" + file);
                Console.WriteLine("DNE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }

            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player.ID;
            magic.receiverGuid = receiver;

            return Newtonsoft.Json.JsonConvert.SerializeObject(magic);
        }

    }
}
