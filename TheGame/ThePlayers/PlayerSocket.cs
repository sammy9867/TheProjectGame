using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ThePlayers
{

    public class PlayerSocket
    {
        private const bool SHOW_JSON = false;

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

        public static void StartClient(string ipAddress, int port)
        {
            try
            {
                //IPAddress ipAddress = IPAddress.Loopback;
                Console.WriteLine("Player IP: " + ipAddress.ToString());
                Console.WriteLine("Player PORT: " + port);


                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port); 
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
                Console.WriteLine("ThePlayers exception Receive():");
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
                        var str = state.sb.ToString();
//                        Console.WriteLine("Read {0} bytes from socket. \nData : {1}", str.Length, str);
                        socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                        return;
                    }
                }
                var content = state.sb.ToString();

                // Here the message is read and we may analize it
                foreach (String _content in content.Split(ETB))
                {
                    if (_content == null || _content == "") continue;
                    if(SHOW_JSON)
                        Console.WriteLine(_content + "\n");
                    AnalyzeMessage(_content);
                }
                // AnalyzeMessage(content);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket handler, String data, Action<string> cb = null)
        {
            // Remove useless white spaces 
            data = Regex.Replace(data, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
            // Add ETB at the end
            data += ETB;            
            // Create bytes
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Sending
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
            
            // After every sent command the player expect the response
            Receive(cb);    // RSP_LBL
        }
        public static void Broadcast(Socket handler, String data, Action<string> cb = null)
        {
            // Remove useless white spaces 
            data = Regex.Replace(data, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
            // Add ETB at the end
            data += ETB;
            // Create bytes
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Sending
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);

            // After every sent command the player does not expect the response
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                
                int bytesSent = client.EndSend(ar);
//                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void AnalyzeMessage(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
            string action = magic.action;

            switch (action.ToLower())
            {
                case "start":
                    {
                        ReadStartGame(json);
                        // Send Discover as soon as the game begins, 
                        // so player knows something... could be move later somewhere else... 
                        Player.SendDiscover = true;
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "state":
                    {
                        ReadDiscover(json);
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "move":
                    {
                        // Nothing is ready for this stage, but once "state" is working successfully, the rest would be easier
                        ReadMove(json);
                        Player.SendDiscover = true;
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "pickup":
                    {
                        ReadPickup(json);
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "test":
                    {

                        //Whenever ReadTestPiece reads false from test response, it resends TestPiece again
                        //Dont know for now if it will ever receive true [that is if that shit is a sham or not]
                        ReadTestPiece(json);
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "destroy":
                    {
                        ReadDestroyPiece(json);
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "place":
                    {
                        ReadPlacePiece(json);
                        Player.Decision decision = Player.MakeMove();
                        SendDecision(decision);
                        if (decision == Player.Decision.KNOWLEDGE_EXCHANGE)
                            Receive();
                        break;
                    }
                case "exchange":
                    {
                        // Receive Authorize Knowledege Exchange
                        // Accept it
                        PlayerRequestHandler.sendAcceptKnowledgeExchange(socket, Player.ID);
                        break;
                    }
                case "send":
                    {
                        // Read KE data
                        ReadKnowledgeExchangeSend(json);
                        SendDecision(Player.MakeMove());
                        break;
                    }
                case "end":
                    {
                        ReadGameOver(json);
                        break;
                        
                    }

                default: break;
                
            }

            // row column
            for (int i = Player.BoardHeight - 1; i >= 0; i--) // row
            {
                string line = "" + i + ". ";
                for (int j = 0; j < Player.BoardWidth; j++)
                { // col
                    Console.Write("" + Player.Board[i, j] + " ");
                    line += " ";
                }
                Console.WriteLine(line);
            }
        }

        private static void ReadKnowledgeExchangeSend(string json)
        {
            JObject magic = JObject.Parse(json);
            JArray fields = (JArray)magic["fields"];
            JObject field = (JObject)fields[0];
            int x = (int) field["x"];
            int y = (int) field["y"];
            JObject val = (JObject)field["value"];
            string contains = (string)val["contains"];
            if (contains.ToLower().Equals("goal"))
            {
                Player.Board[y, x] = Player.BoardCell.GL;
            }
            Console.WriteLine("Player got to know about "+Player.Board[y,x] + " on row="+y+" col="+x);
        }
        
        private static void ReadGameOver(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
            string result = magic.result;
            Console.WriteLine("Team " + result + " wins!!!");
            Console.ReadKey();
        }

        private static void SendDecision(Player.Decision decision)
        {
            Console.WriteLine("Player decided: " + decision);
           //Thread.Sleep(1000);
            switch (decision)
            {
                case Player.Decision.MOVE_NORTH:  SendMove("N"); return;
                case Player.Decision.MOVE_SOUTH:  SendMove("S"); return;
                case Player.Decision.MOVE_WEST:   SendMove("W"); return;
                case Player.Decision.MOVE_EAST:   SendMove("E"); return;

                case Player.Decision.PICKUP_PIECE: SendPickup(); return;
                case Player.Decision.TEST_PIECE: SendTestPiece(); return;
                case Player.Decision.DESTROY_PIECE: SendDestroyPiece(); return;
                case Player.Decision.PLACE_PIECE: SendPlacePiece(); return;
                case Player.Decision.DISCOVER: SendDiscover(); return;
                case Player.Decision.KNOWLEDGE_EXCHANGE: SendKnowledgeExchange(); return;

            }
        }

        private static void ReadStartGame(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
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
            for (int i = 0; i < magic.teamGuids.Count; i++)
            {
                Player.Mates.Add( (string) magic.teamGuids[i]);
            }
            Player.BoardWidth = magic.board.width;
            Player.BoardTaskHeight = magic.board.tasksHeight;
            Player.BoardGoalHeight = magic.board.goalsHeight;
            Player.Board = new Player.BoardCell[Player.BoardHeight, Player.BoardWidth];

        
            Console.WriteLine(Player.X + " " + Player.Y);
            Console.WriteLine(Player.BoardWidth + " " + Player.BoardHeight);
            Console.WriteLine(Player.BoardHeight - Player.BoardGoalHeight);

            for (int i = 0; i < Player.BoardGoalHeight; i++)
                for (int j = 0; j < Player.BoardWidth; j++)
                {
                    Player.Board[i, j] = Player.BoardCell.GC;
                    Player.Board[Player.BoardHeight - 1 - i, j] = Player.BoardCell.GC;
                }

            


            Player.Board[Player.Y, Player.X] = Player.BoardCell.ME;  // row col
            Player.current = Player.Y < Player.BoardGoalHeight || Player.Y > Player.BoardHeight - Player.BoardGoalHeight ? Player.BoardCell.GC : Player.BoardCell.EC;

            Console.WriteLine("Player " + Player.ID + "  [row,col] " + Player.current);
            for (int i = Player.BoardHeight - 1; i >= 0; i--) // row
            {
                string line = "" + i + ". ";
                for (int j = 0; j < Player.BoardWidth; j++)
                { // col
                    Console.Write("" + Player.Board[i, j] + " ");
                    line += " ";
                }
                Console.WriteLine(line);
            }

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
            Console.WriteLine("DiscoverResponse:");
            if(SHOW_JSON)   
                Console.WriteLine(json);
            JObject jobject = JObject.Parse(json);
            string result = (string) jobject["result"];
            if (result.ToLower().Equals("denied"))
            {
                Player.SendDiscover = true;
                return;
            }

            // Update coordinates 
            JObject jscope = (JObject)jobject["location"];
            Player.X = (int)jscope["x"];   // shall we check for correctness first ?
            Player.Y = (int)jscope["y"];   // shall we check for correctness first ?

            JArray jfields = (JArray)(jobject["fields"]);
            // Initialy Neighbors blocked
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Player.Neighbors[i, j] = Player.NeighborStatus.BL;

            for (int i = 0; i < jfields.Count; i++)
            {
                JObject jfield = (JObject)jfields[i];
                int  x = (int) jfield["x"];
                int  y = (int) jfield["y"];
                JObject value = (JObject)jfield["value"];
                int manhattanDistance = (int)value["manhattanDistance"];
                string contains = (string) value["contains"];
                string timestamp = (string) value["timestamp"];
                string userGuid = (string) value["userGuid"];
                Player.NeighborStatus status;
                Player.BoardCell curr = Player.Board[y,x];  // row col
                //By [row, col]
                int dx = Player.X - x;
                int dy = Player.Y - y;
                if (1 - dy < 0 || 1 - dy > 2)
                {
                    /// TODO
                    Console.WriteLine("debug");
                }
                if (1 - dx < 0 || 1 - dx > 2)
                {
                    /// TODO
                    Console.WriteLine("debug");
                }
                Console.WriteLine("Neighbors["+(1 - dy)+", "+(1 - dx)+"]");
                status = Player.Neighbors[1 - dy, 1 - dx] = Player.NeighborStatus.BL;  // row col.

                switch (contains)
                {
                    case "goal":
                        status = Player.NeighborStatus.DG;
                        Player.Board[y, x] = Player.BoardCell.GL;
                        break;
                    case "empty":
                        if (userGuid == null)
                        {
                            // Free cell
                            status = curr == Player.BoardCell.GL ? Player.NeighborStatus.DG : Player.NeighborStatus.FR;
                            //if ((Player.Board[y, x] & Player.BoardCell.PL) == Player.BoardCell.PL)
                            //    Player.Board[y, x] = Player.Board[y, x] & (~Player.BoardCell.PL);
                        }
                        else
                        {
                            if (Player.ID.ToLower().Equals(userGuid.ToLower()))
                            { // the player itself
                                status = curr == Player.BoardCell.GL ? Player.NeighborStatus.DG : Player.NeighborStatus.FR;
                                status = curr == Player.BoardCell.PC ? Player.NeighborStatus.PC : Player.NeighborStatus.FR;
                                if (curr == Player.BoardCell.SH)
                                    status = Player.hasPiece ? Player.NeighborStatus.BL : Player.NeighborStatus.FR; 
                                break;
                            }
                            // Player is staying 
                            status = Player.NeighborStatus.BL;
                            //Player.Board[y, x] = Player.Board[y, x] | Player.BoardCell.PL;
                        }
                        break;
                    case "piece":
                       // check if we know there is a sham
                        if (Player.Board[y, x] == Player.BoardCell.SH)
                            // if yes, set BLOCKED
                            status = (Player.hasPiece) ? Player.NeighborStatus.BL : Player.NeighborStatus.FR;
                        else
                        {
                            // set PIECE otherwise
                            status = (Player.hasPiece) ? Player.NeighborStatus.BL : Player.NeighborStatus.PC;
                            Player.Board[y, x] = Player.BoardCell.PC;
                        }

                        break;
                }
                if (status == Player.NeighborStatus.FR && (Player.Board[y, x] & Player.BoardCell.GC)==Player.BoardCell.GC)
                    status = Player.NeighborStatus.GA;
                Player.Neighbors[1 - dy, 1 - dx] = status;  // row col.

            }

            Console.WriteLine("After Discover:");
            for (int i = Player.BoardHeight - 1; i >= 0; i--) // row
            {
                string line = "" + i + ". ";
                for (int j = 0; j < Player.BoardWidth; j++)
                { // col
                    Console.Write("" + Player.Board[i, j] + " ");
                    line += " ";
                }
                Console.WriteLine(line);
            }

            Console.WriteLine("After Discover Neighboors:" + Player.current);
            for (int i = 2; i >= 0; i--)
            {
                for (int j = 0; j < 3; j++)
                    Console.Write("" + Player.Neighbors[i, j] + " "); // row col
                Console.WriteLine("");
            }

        }

        private static void SendMove(string direction)
        {
            Player.ApplyiedDirection = direction;
            // Send Move action
            PlayerRequestHandler.sendMove(socket, direction);
            // Receive Move Reponse
            // Receive();  check Send() method, it calls Receive()
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadMove(string json)
        {
            string direction = Player.ApplyiedDirection;
            Player.ApplyiedDirection = null;

            Console.WriteLine("MoveResponse: "+direction);
            Console.WriteLine(json);

            JObject jobject = JObject.Parse(json);
            string result = (string)jobject["result"];
            if (result.ToLower().Equals("denied"))
            {
                // TODO: Maaaan
                // SendMove(); // Repeat Move ??
                Player.SendDiscover = true; // Re-Discover 
                return;
            }
            int old_x = Player.X;
            int old_y = Player.Y;
            Player.Board[old_y, old_x] = Player.current;


            Player.DoMove(direction);

            int new_x = Player.X;
            int new_y = Player.Y;
            Player.current = Player.Board[new_y, new_x];
            Player.Board[new_y, new_x] = Player.BoardCell.ME;


           for (int i = Player.BoardHeight - 1; i >=0; i--) // row
            {
                string line = "" + i + ". ";
                for (int j = 0; j < Player.BoardWidth; j++)
                { // col
                    Console.Write("" + Player.Board[i, j] + " ");
                    line += " ";
                }
                Console.WriteLine(line);
            }


        }
        
        private static void SendPickup()
        {
            // send Pickup action
            PlayerRequestHandler.sendPickup(socket);

            // After every send command, player expect the response
            // Receive() is called  at the end of Send() method 
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadPickup(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
            string result = magic.result;
            if (result.ToLower().Equals("denied"))
            {
                Player.Piece = null;
                Player.Board[Player.Y, Player.X] = Player.current = Player.BoardCell.EC; 
                /* Doing nothing force a player to re-try pickup */ 
                return;
            }
            /* Whenever we pickup an unchecked piece we assume it is not a sham */
            Player.Piece = new Piece();
            Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.EC;
        }

        private static void SendTestPiece()
        {
            // send TestPiece action
            PlayerRequestHandler.sendTestPiece(socket);

            // After every send command, player expect the response
            // Receive() is called  at the end of Send() method 
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadTestPiece(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
            string result = magic.result;
            
            if (result.ToLower().Equals("denied"))
            {
                Player.Piece = null;
                return;
            }
            if(magic.test == "true") //iF a SHAM
            {
                Player.Piece.isSham = true;
                Player.Piece.isTested = true;
                Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.SH;
            }
            else 
            {
                Player.Piece.isSham = false;
                Player.Piece.isTested = true;
                Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.EC;
            }
        }

        private static void SendDestroyPiece()
        {
            // send DestroyPiece action
            PlayerRequestHandler.sendDestroyPiece(socket);

            // After every send command, player expect the response
            // Receive() is called  at the end of Send() method 
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadDestroyPiece(string json)
        {
            Console.WriteLine("Reading Destroy Piece");
            dynamic magic = JsonConvert.DeserializeObject(json);
            string result = magic.result;
            if (result.ToLower().Equals("denied"))
            {
                // TODO: 
                return;
            }
            /* piece is destroyed */
            Player.Piece = null;
            Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.EC;
        }

        private static void SendPlacePiece()
        {
            // send DestroyPiece action
            PlayerRequestHandler.sendPlacePiece(socket);

            // After every send command, player expect the response
            // Receive() is called  at the end of Send() method 
            // use ctrl+F to find lable RSP_LBL
        }
        private static void ReadPlacePiece(string json)
        {
            Console.WriteLine("Reading Place Piece\n");
            dynamic magic = JsonConvert.DeserializeObject(json);
            string result = magic.result;

            Player.Piece = null;

            if (result.ToLower().Equals("denied"))
                return;
            

            // Does not matter anymore
            //string consequence = magic.consequence;
            //if (consequence.ToLower().Equals("correct"))
            //{
            //    // placed on a goal.  discover goal
            //    Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.GL;
            //    // Knowledge exchange 
            //    //Player.KnowledgeExchange = true;
            //}
            //if (consequence.ToLower().Equals("meaningless"))
            //{
            //    // placed on a goal.  discover non-goal
            //    Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.GL;
            //    // Knowledge exchange 
            //    //Player.KnowledgeExchange = true;
            //}

            Player.current = Player.Board[Player.Y, Player.X] = Player.BoardCell.GL;
            /* Knowledge exchange */
            Player.KnowledgeExchange = true;
        }

        /* *
         * Player sends KE after placing a piece on the Goal Area notifying others about discoverd goal/non-goal
         */
        private static void SendKnowledgeExchange()
        {
            /* sends authorise */
            foreach (string p in Player.Mates)
                PlayerRequestHandler.sendAuthorizeKnowledgeExchange(socket, Player.ID, p);

            /* sends data */
            foreach (string p in Player.Mates)
            {
                string json = PlayerRequestHandler.stringKnowledgeExchangeSend(socket, Player, p);
                JObject magic = JObject.Parse(json);
                JField jField = new JField
                {
                    x = "" + Player.X,
                    y = "" + Player.Y
                };
                jField.value = new JFieldValue();
                jField.value.manhattanDistance = null;
                jField.value.contains = "goal";
                jField.value.timestamp = GetTimestamp().ToString();
                jField.value.userGuid = null;
                var jArray = new List<JField>();
                jArray.Clear();
                jArray.Add(jField);
                magic["fields"] = (JArray)JToken.FromObject(jArray);

                Broadcast(socket, 
                    JsonConvert.SerializeObject(magic));

            }

            Player.KnowledgeExchange = false;
            return;
        }

        public static long GetTimestamp()
        {
            // TODO:
            return 777;
        }

    }

    public class JField
    {
        public string x;
        public string y;
        public JFieldValue value;
    }

    public class JFieldValue
    {
        public string manhattanDistance;
        public string contains;
        public string timestamp;
        public string userGuid;
    }

}
