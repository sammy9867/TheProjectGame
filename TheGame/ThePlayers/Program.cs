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
            foreach(Object ibj in args)
                Console.WriteLine(ibj);
            if(args.Length != 1)
            {
                Usage();
                return -1;
            }
            if (!args[0].ToLower().Equals("red") && !args[0].ToLower().Equals("blue"))
            {
                Usage();
                return -1;
            }
            Console.WriteLine("Player ["+args[0]+"] has started");

            // Create Player object
            Player player = new Player
            {
                playerID = ""+(new Random()).Next(),
                Team = args[0].ToLower().Equals("red") ? Player.TeamColor.RED : Player.TeamColor.BLUE
            };

            // Initialize player 
            PlayerSocket.Player = player;

            // Start Communication with CS
            PlayerSocket.StartClient();

            Console.WriteLine("Player "+player.playerID+" terminated");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }

        private static void Usage()
        {
            Console.WriteLine("Invalid arguments");
            Console.WriteLine("Only one arg to indicate the Team");
            Console.ReadKey();
        }
    }
}
