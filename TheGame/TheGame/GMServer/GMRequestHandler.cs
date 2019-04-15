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
        public static string Response;
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
            allDone.Set();
        }
    }
}
