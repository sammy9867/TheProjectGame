using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThePlayers
{

    public class PlayerSocket
    {
        private const int port = 11000;
        private const char ETB = (char)23;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static String response = String.Empty;

        public static Player Player;
        public static Socket socket;

        public static void StartClient()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Loopback;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));

                socket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());
   
                connectDone.Set();
              
                // Request to Join the game
                PlayerRequestHandler.sendJoinGame(socket);
                // Response to join the game
                Receive();
                // Receive BeginGame message
                Receive();

                Console.WriteLine("Player begins the game");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Receive(Action<string> cb = null)
        {
            try
            {
                StateObject state = new StateObject();
                state.cb = cb;

                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;

                int bytesRead = socket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb = state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    if (state.sb.ToString().IndexOf(ETB) < 0)
                    {
                        socket.BeginReceive(state.buffer, bytesRead, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                        return;
                    }
                }
                var content = state.sb.ToString();
                content = content.Replace(ETB, ' ');
                Console.WriteLine("Read {0} bytes from socket. \nData : {1}",
                    content.Length, content);

                // Here the message is read and we may analize it
                AnalizeMessage(content);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket handler, String data, Action<string> cb = null)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data + ETB);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
            
            // After every sent command the player expect the response
            Receive(cb);    // RSP_LBL
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void AnalizeMessage(string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string action = magic.action;

            switch (action.ToLower())
            {
                case "begin":
                    {
                        ReadStartGame(json);
                        // Send Discover as soon as the game begins, 
                        // so player knows something... could be move later somewhere else... 
                        SendDiscover();
                        break;
                    }
                case "state":
                    {
                        ReadDiscover(json);
                        break;
                    }
                case "move":
                    {   // Nothing is ready for this stage, but once "state" is working successfully, the rest would be easier
                        string direction = magic.direction;
                        Console.WriteLine("Direction: " + direction);
                        break;
                    }
                default: break;
                
            }

        }
        private static void ReadStartGame(string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string team = magic.team;
            string role = magic.role;
            string x = magic.location.x;
            string y = magic.location.y;
            Player.Row = Int32.Parse(y);
            Player.Column = Int32.Parse(x);
            Player.Team =
                team.ToLower().Equals("red") ? Player.TeamColor.RED : Player.TeamColor.BLUE;
            Player.role = 
                role.ToLower().Equals("member") ? Player.Role.MEMBER : Player.Role.LEADER;
            Player.TeamSize = magic.teamSize;
            Player.Mates = new List<string>();

            string tmp = magic.teamGuids;
            Player.Mates = 
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(tmp);

            Player.BoardWidth = magic.board.width;
            Player.BoardTaskHeight = magic.board.tasksHeight;
            Player.BoardGoalHeight = magic.board.goalsHeight;
            Player.Board = new Player.BoardCell[Player.BoardHeight, Player.BoardWidth];

            Console.WriteLine("Player " + Player.ID + "  [row,col]");
            Console.WriteLine("" + Player.Row + " " + Player.Column);
            foreach (string p in Player.Mates)
                Console.WriteLine("+"+p);
            Console.WriteLine();
        }

        private static void SendDiscover()
        {
            // send Discover action
            PlayerRequestHandler.sendDiscover(socket);
            
            // After every send command, player expect the response
            // Receive() is called  at the end of Send() method 
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadDiscover(string json)
        {
            Console.WriteLine("DiscoverResponce:");
            Console.WriteLine(json);
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            string result = magic.result;
            if (result.ToLower().Equals("denied"))
            {
                SendDiscover(); // Repeat Discover ??
                return;
            }

            // Update coordinates 
            Player.X = magic.scope.x;   // shall we check for correctness first ?
            Player.Y = magic.scope.y;   // shall we check for correctness first ?

            for (int i = 0; i < magic.fields.Count; i++)
            {
                int col = magic.fields[i].x;
                int row = magic.fields[i].y;
            }
        }

        private static void SendMove()
        {
            // Send Move action
            PlayerRequestHandler.sendMove(socket);
            // Receive Move Reponse
            // Receive();  check Send() method, it calls Receive()
        }
    }
}
