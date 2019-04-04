using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePlayers
{
    public class StateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public Action<string> cb = null;
    }

    class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("ThePlayer has started");
            PlayerSocket.StartClient();

            Console.ReadLine();
            return 0;
        //    start();
        }

      

        private static void start()
        {
            bool flag = true;

            while (flag)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "add":
                        Console.WriteLine("new player shall be added");
                        break;
                    case "exit":
                        Console.WriteLine("Thank you for using our app !");
                        flag = false;
                        break;
                    default:
                        Console.WriteLine("<"+command + "> command does not exist");
                        break;
                }

            }
        }
    }
}
