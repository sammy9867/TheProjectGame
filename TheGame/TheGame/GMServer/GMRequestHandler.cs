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
                
        
        /** 
         * README
         * Ok, before you start screaming, since running too many threads causes too many problems,
         * I have combined GMSocket and Main thread.
         * So, we may call Send() Receive() and have GUI, access to Board object,
         * and methods to write report file, and do the actual GM job, soooooo
         * METHODS JUST RETURN DUMMY JSON TO SEND
         */

        public static string SendSetUpGame()
        {
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
            return (json);
        }

        // NO NEED
        //internal static void ConnectPlayer(GMSocket GMSocket, out Player player)
        //{
        //    GMSocket.Receive();
        //    player = null;

        //    dynamic magic = JsonConvert.DeserializeObject(GMSocket.SyncResponse);
        //    string action = magic.action;
        //    string team = magic.preferredTeam;

        //    if (!action.ToLower().Equals("connect"))
        //        return;
        //    if (!team.ToLower().Equals("red") && !team.ToLower().Equals("blue"))
        //        return;

        //    player = new Player();
        //    player.playerID = magic.userGuid;
        //    player.Team = team.ToLower().Equals("red") ? 
        //        Team.TeamColor.RED : Team.TeamColor.BLUE ;

        //}

        /* Returns JSON for Successfull Joining Notification */
        internal static string ConnectPlayerOK(Player player)
        {
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

            return (JsonConvert.SerializeObject(magic));
//          
        }
        /* Returns JSON for Failure Joining Notification */
        internal static string ConnectPlayerDeny(Player player)
        {
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

            return JsonConvert.SerializeObject(magic);
        }

        /* Returns JSON for Begin Game Notification */
        internal static string BeginGame( 
            Player player, List<Player> members, Player leader)
        {
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

            return  (JsonConvert.SerializeObject(magic));
        }

        //TODO: 2nd Communication phase
        internal static void ResponseForMove(Player player)
        {

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

        /* Returns JSON for Discovery Response */
        internal static string ResponseForDiscover(Player player)
        {
            string file = @"..\..\JSONs\Response\DiscoverResponse.json";
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

            return JsonConvert.SerializeObject(magic);

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
