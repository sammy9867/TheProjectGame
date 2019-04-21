using System;
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
            magic.userGuid = PlayerSocket.Player.playerID;
            magic.preferredTeam = PlayerSocket.Player.Team == Player.TeamColor.RED ? "red" : "blue";

            Console.WriteLine("Sending JoinGame.json to ComServer.....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));

        }

        public static void sendMove(Socket handler)
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
            magic.userGuid = PlayerSocket.Player.playerID;
            //CHANGE follow:
            magic.direction = PlayerSocket.Player.Team == Player.TeamColor.RED ? "S" : "N";

            Console.WriteLine("Sending Move.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));

        }

        //TODO: 2nd Communication phase
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
            magic.userGuid = PlayerSocket.Player.playerID;
            //CHANGE FOLLOWING
            //magic.scope.x = PlayerSocket.Player.Team == Player.TeamColor.RED ? "red" : "blue";
           // magic.scope.y = PlayerSocket.Player.Team == Player.TeamColor.RED ? "red" : "blue";

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
            magic.userGuid = PlayerSocket.Player.playerID;
          
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
            magic.userGuid = PlayerSocket.Player.playerID;

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
            magic.userGuid = PlayerSocket.Player.playerID;

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
            magic.userGuid = PlayerSocket.Player.playerID;

            Console.WriteLine("Sending Destroy.json....\n");
            PlayerSocket.Send(handler,
                JsonConvert.SerializeObject(magic));
        }


        //After 2nd communication phase done, commence 3rd communication phase: KnowledgeExchange
        public static void sendAuthorizeKnowledgeExchange() { }
        public static void sendRejectKnowledgeExchange() { }
        public static void sendAcceptKnowledgeExchange() { }
        public static void ResponseForKnowledgeExchange() { }

    }
}
