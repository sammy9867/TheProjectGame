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


    }
 }
