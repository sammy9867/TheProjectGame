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
            Console.WriteLine("Player has started");

            // Create Player object
            Player player = new Player();

            // Initialize player 
            PlayerSocket.Player = player;

            // Start Communication with CS
            PlayerSocket.StartClient();

            Console.WriteLine("Player "+player.playerID+" terminated");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }

    }
}
