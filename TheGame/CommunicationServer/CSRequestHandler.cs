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
            Console.WriteLine(data);

            Server.Send(gMSocket, data);
        }

        internal static void ConnectPlayerConfirmation(String data, Socket destPlayer)
        {
            Console.WriteLine("ConnectPlayer");
            Console.WriteLine(data);

            Server.Send(destPlayer, data);
        }
    }
}
