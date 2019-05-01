﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using TheGame.Model;
using Newtonsoft.Json.Linq;

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
         * https://www.newtonsoft.com/json/help/html/ModifyJson.htm
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
        public static string ConnectPlayerOK(Player player)
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
        public static string ConnectPlayerDeny(Player player)
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
        public static string BeginGame( 
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
//            dynamic magic = JsonConvert.DeserializeObject(json);
            JObject jObject = JObject.Parse(json);

            jObject["userGuid"] = player.playerID;
            jObject["team"] = player.Team == Team.TeamColor.RED ? "red" : "blue";
            jObject["role"] =  player.role == Player.Role.LEADER ? "leader" : "member";
            jObject["teamSize"] = "" + members.Count;

            JArray teamGuids = (JArray)jObject["teamGuids"];
            teamGuids.Clear();
            teamGuids.Add(leader.playerID);

            int i = 1;
            foreach (var p in members)
            {
                if (p.playerID != leader.playerID)
                    teamGuids.Add(p.playerID);
            }

            JObject location = (JObject)jObject["location"];
            location["x"] = "" + player.Column;
            location["y"] = "" + player.Row;

            JObject board = (JObject)jObject["board"];
            board["width"] = "" + Board.Width;
            board["tasksHeight"] = "" + Board.TaskHeight;
            board["goalsHeight"] = "" + Board.GoalHeight;

            string result = jObject.ToString();
            Console.WriteLine(result);
            return  result;  // TODO?
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
        public static string ResponseForDiscover(Player player)
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
