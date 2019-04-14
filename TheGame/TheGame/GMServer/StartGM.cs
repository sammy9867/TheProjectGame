using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TheGame.GMServer
{
    class StartGM
    {
        [STAThread]
        public static void Main(string[] args)
        {
            //This is app startup.
            //For now, Game master will connect to server and start the game in the background.
            Console.WriteLine("Game master has started...\n");
            GMSocket.StartClient();

            var app = new App();
            app.InitializeComponent();
            app.Run();
          

        }

    }
}
