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
using Newtonsoft.Json.Linq;

namespace TheGame.GMServer
{
    public static class GMRequestHandler
    {
        public static string Response { get; set; }
        public static ManualResetEvent allDone
            = new ManualResetEvent(false);

        private static void initFilejSON()
        {
            // create a file object
            // create file itself if it does not exist
            string newFileName = @"..\..\Configfile\JSONLog.txt";


            if (!File.Exists(newFileName))
            {
                string clientHeader = $"\"Message\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }
            else if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
                string clientHeader = $"\"Message\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }

        }

        private static bool insertIntoConfigJSON(string m)
        {
            try
            {
                string line = $"\"{m}\"{Environment.NewLine}";
                File.AppendAllText(@"..\..\Configfile\JSONLog.txt", line);
                return true;
            }
            catch (Exception ee)
            {
                string temp = ee.Message;
                return false;
            }
        }


        /** 
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
        public static string BeginGame(Player player, List<Player> members, Player leader)
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
            JObject jObject = JObject.Parse(json);

            jObject["userGuid"] = player.playerID;
            jObject["team"] = player.Team == Team.TeamColor.RED ? "red" : "blue";
            jObject["role"] =  player.role == Player.Role.LEADER ? "leader" : "member";
            jObject["teamSize"] = members.Count;

            JArray teamGuids = (JArray)jObject["teamGuids"];
            teamGuids.Clear();
            teamGuids.Add(leader.playerID);

            foreach (var p in members)
            {
                if (p.playerID != leader.playerID)
                    teamGuids.Add(p.playerID);
            }
            JObject location = (JObject)jObject["location"];
            location["x"] = player.Column;
            location["y"] = player.Row;

            JObject board = (JObject)jObject["board"];
            board["width"] = Board.Width;
            board["tasksHeight"] =Board.TaskHeight;
            board["goalsHeight"] = Board.GoalHeight;

            initFilejSON();
            insertIntoConfigJSON(jObject.ToString());
            return jObject.ToString();
        }


        public static string ResponseForMove(Player player)
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
            return JsonConvert.SerializeObject(magic);
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
        public static string ResponseForPickUp(Player player)
        {
            string file = @"..\..\JSONs\Response\PickUpResponse.json";
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
        public static string ResponseForTestPiece(Player player)
        {
            string file = @"..\..\JSONs\Response\TestPieceResponse.json";
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

        public static string ResponseForDestroyPiece(Player player)
        {
            string file = @"..\..\JSONs\Response\DestroyPieceResponse.json";
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

        public static string ResponseForPlacePiece(Player player)
        {
            string file = @"..\..\JSONs\Response\PlacePieceResponse.json";
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


        public static string SendGameOver(Team.TeamColor avengers)
        {
            string file = @"..\..\JSONs\GameOver.json";
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
            magic.result = avengers == Team.TeamColor.RED ? "red" : "blue";

            return JsonConvert.SerializeObject(magic);
        }


    }
}
