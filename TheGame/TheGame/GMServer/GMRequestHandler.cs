using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using TheGame.Model;

namespace TheGame.GMServer
{
    public static class GMRequestHandler
    {
        public static string Response { get; set; }
        public static ManualResetEvent allDone
            = new ManualResetEvent(false);
                
        //Sending ConfirmSetUpGame JSON to to client on connection
        public static void SendSetUpGame(GMSocket gmSocket)
        {
            Socket handler = gmSocket.socket;
            string file = @"..\..\JSONs\SetUpGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DONE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            gmSocket.Send(handler, json);
            gmSocket.sendDone.WaitOne();

            gmSocket.Receive();
            
            Response = gmSocket.Response;
            gmSocket.Response = "";
            allDone.Set();
        }

        internal static void ConnectPlayer(GMSocket GMSocket, out Player player)
        {
            GMSocket.Receive();
            player = null;

            dynamic magic = JsonConvert.DeserializeObject(GMSocket.Response);
            string action = magic.action;
            string team = magic.preferredTeam;

            if (!action.ToLower().Equals("connect"))
                return;
            if (!team.ToLower().Equals("red") && !team.ToLower().Equals("blue"))
                return;

            player = new Player();
            player.playerID = magic.userGuid;
            player.Team = team.ToLower().Equals("red") ? 
                Team.TeamColor.RED : Team.TeamColor.BLUE ;
            
        }

        internal static void ConnectPlayerOK(GMSocket gmSocket, Player player)
        {
            Socket handler = gmSocket.socket;
            string file = @"..\..\JSONs\ConfirmJoiningGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DONE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            magic.userGuid = player.playerID;

            gmSocket.Send(handler, JsonConvert.SerializeObject(magic));
//            gmSocket.sendDone.WaitOne();
        }
        
        internal static void ConnectPlayerDeny(GMSocket gmSocket, Player player)
        {
            Socket handler = gmSocket.socket;
            string file = @"..\..\JSONs\RejectJoiningGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DONE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            magic.userGuid = player.playerID;

            gmSocket.Send(handler, JsonConvert.SerializeObject(magic));
//            gmSocket.sendDone.WaitOne();
        }

        internal static void BeginGame(GMSocket gmSocket, 
            Player player, List<Player> members, Player leader)
        {
            Socket handler = gmSocket.socket;
            string file = @"..\..\JSONs\BeginGame.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DONE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player.playerID;
            magic.team = player.Team == Team.TeamColor.RED ? "red" : "blue";
            magic.role = player.role == Player.Role.LEADER ? "leader" : "member";
            magic.teamSize = "" + members.Count;

            string[] ids = new string[members.Count];
            ids[0] = leader.playerID;
            int i = 1;
            foreach (var p in members)
            {
                if (p.playerID != leader.playerID)
                    ids[i++] = p.playerID;
            }

            magic.teamGuids = JsonConvert.SerializeObject(ids) ;

            
            magic.location.x = "" + player.column;
            magic.location.y = "" + player.row;

            magic.board.width = "" + Board.Width;
            magic.board.tasksHeight = "" + Board.TaskHeight;
            magic.board.goalsHeight = "" + Board.GoalHeight;

            gmSocket.Send(handler, JsonConvert.SerializeObject(magic));
        }

        //TODO: 2nd Communication phase
        internal static void ResponseForMove(GMSocket gmSocket, Player player)
        {
            Socket handler = gmSocket.socket;
            string file = @"..\..\JSONs\Response\MoveResponseAcceptance.json";
            string json = "";
            if (!File.Exists(file))
            {
                Console.WriteLine("DONE\n");
            }
            else
            {
                json = File.ReadAllText(file, Encoding.ASCII);
            }
            dynamic magic = JsonConvert.DeserializeObject(json);
            magic.userGuid = player.playerID;
            magic.manhattanDistance = player.Team == Team.TeamColor.RED ? player.goDown() : player.goUp();

        }


        internal static void ResponseForDiscover()
        {

        }
        internal static void ResponseForPickUp()
        {

        }
        internal static void ResponseForTestPiece()
        {

        }
        internal static void ResponseForDestroyPiece()
        {

        }
        internal static void ResponseForPlacePiece()
        {

        }
        internal static void sendGameOver()
        {

        }   


        //After 2nd communication phase done, commence 3rd communication phase: KnowledgeExchange

            


    }
}
