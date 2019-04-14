using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.IO;
using System.Threading;
namespace TheGame.GMServer
{
    public static class GMRequestHandler
    {
        //Sending ConfirmSetUpGame JSON to to client on connection
        public static void sendSetUpGame(Socket handler)
        {
            string file = @"..\..\JSONs\SetUpGame.json";
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
            Console.WriteLine("Sending SetUpGame.json.....\n");
            GMSocket.Send(handler, action);

        }
    }
}
