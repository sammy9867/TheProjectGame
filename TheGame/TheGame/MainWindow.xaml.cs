using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheGame.Model;
using TheGame.GMServer;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace TheGame
{
    public partial class MainWindow : Window
    {
        private Board board;
        private Team RedTeam;
        private Team BlueTeam;

        private const int port = 11000;
        private const char ETB = (char)23;

        // ManualResetEvent instances signal completion.
        public ManualResetEvent connectDone =
            new ManualResetEvent(false);
        public ManualResetEvent sendDone =
            new ManualResetEvent(false);
        public ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        public Socket GMSocket;

        // Boolean to indicate if the game is over
        private bool endgame;
        private bool startgame;
        
        public MainWindow()
        {

            InitializeComponent();
            Console.WriteLine("Game master has started.");
            RedTeam = BlueTeam = null;
            endgame = false;
            startgame = true;

            board = new Board();

            initFile();
            loadBoard();

            UpdateBoard();

            // offset before running communication routine
            var dueTime = TimeSpan.FromSeconds(1);

            // run separate thread for communication routine
            //            RunAsync(CommunicationRoutine, dueTime, CancellationToken.None);

            var interval = TimeSpan.FromMilliseconds(10); // FromSeconds(0);
            RunPeriodicAsync(CommunicationRoutine, dueTime, interval, CancellationToken.None);
        }

       
        #region Start Socket
        /* Start Socket */
        private void StartSocket()
        {
            Console.WriteLine("GM Socket started.");
            // Start client sync
            StartClient();

            // Sending SetUp reuest and getting response
            string json = GMRequestHandler.SendSetUpGame();
            Send(GMSocket, json);
            sendDone.WaitOne();
            sendDone.Reset();

            // Receive response and analize it in StartGameResponse method
            Receive(StartGameResponse);
        }
        private void StartGameResponse(string json)
        {
            /* Response to Start Game request, always OK for now */
            dynamic magic =
                JsonConvert.DeserializeObject(json);
            string action = magic.action;
            string result = magic.result;
            Console.WriteLine("action: " + action);
            Console.WriteLine("result: " + result);
            sendDone.Set();
            if (result.ToLower().Equals("denied"))
            {
                Console.WriteLine("Game has been denied");
                MessageBox.Show("Game has been denied",
                    "Error");
                this.Close();
            }
        }
        #endregion
        #region Connect Players
        /* Metod to connect players, while there is at least one available slot */
        private void ConnectPlayers()
        {
            if (null == GMSocket)
            {
                Console.WriteLine("Socket is null");
                MessageBox.Show("Socket is null", "Error");
                this.Close();
            }

            Console.WriteLine("Expect players");
            while(board.RedTeam.NumOfPlayers  < Board.MaxNumOfPlayers || 
                  board.BlueTeam.NumOfPlayers < Board.MaxNumOfPlayers)
            {
                // Received Message is analized by ConnectPlayer method
                Receive(ConnectPlayer);
                connectDone.WaitOne();  // wait untill ConnectPlayer completes
                connectDone.Reset();    // reset handler

                // ConsoleWrite[Line] and GUI are not working properly 
                // be awere that the same objects cannot be used 
                // by multiple threads simultaneously without safty
                string line = ("Players connected " + RedTeam.NumOfPlayers + "|" +
                                    BlueTeam.NumOfPlayers + "|" + Board.MaxNumOfPlayers);
                Console.WriteLine(line);

            }
            Console.WriteLine("All Players Connected");

        }
        private void ConnectPlayer(string json)
        {
            dynamic magic = JsonConvert.DeserializeObject(json);
            string action = magic.action;
            string team = magic.preferredTeam;

            if (!action.ToLower().Equals("connect"))
                return;
            if (!team.ToLower().Equals("red") && !team.ToLower().Equals("blue"))
                return;

            Player player = new Player();
            player.playerID = magic.userGuid;
            player.Team = team.ToLower().Equals("red") ?
                Team.TeamColor.RED : Team.TeamColor.BLUE;
            player.Neighbors = new Player.NeighborStatus[3, 3];
            if (player.Team == Team.TeamColor.RED)
            {
                if (board.RedTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                {
                    Send(GMSocket, GMRequestHandler.ConnectPlayerDeny(player));
                    return;
                }
                if (board.RedTeam.leader == null)
                    board.RedTeam.leader = player;
                player.row = 1; // TODO: Update the row
                player.column = RedTeam.members.Count;
                board.RedTeam.members.Add(player);
            }
            else
            {
                if (board.BlueTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                {
                    Send(GMSocket, GMRequestHandler.ConnectPlayerDeny(player));
                    return;
                }
                if (board.BlueTeam.leader == null)
                    board.BlueTeam.leader = player;
                player.row = Board.Height - 1;  // TODO: Update the row
                player.column = BlueTeam.members.Count;
                board.BlueTeam.members.Add(player);
            }
            Send(GMSocket, GMRequestHandler.ConnectPlayerOK(player));

            connectDone.Set();
        }
        #endregion
        #region Begion Game
        /* Send all refistered players notification that the game has started */
        private void BeginGame()
        {
            Console.WriteLine("");
            Console.WriteLine("Begin the game for Blue Team");
            foreach (var p in BlueTeam.members)
            {
                Send(GMSocket, GMRequestHandler.BeginGame( p, BlueTeam.members, BlueTeam.leader) );
            }
            Console.WriteLine("Begin the game for Red Team");
            foreach (var p in RedTeam.members)
            {
                Send(GMSocket, GMRequestHandler.BeginGame(p, RedTeam.members, RedTeam.leader));
            }
            Console.WriteLine("Started");
        }
        #endregion
        #region File Report
        /** File Report most probably will be changed since we have multiple threads **/
        private void initFile()
        {
            // create a file object
            // create file itself if it does not exist
            string newFileName = "Configfile/report.csv";

            if (!File.Exists(newFileName))
            {
                string clientHeader = $"\"Type\",\"Timestamp\",\"Player ID\",\"Colour\",\"Role\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }
            else if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
                string clientHeader = $"\"Type\",\"Timestamp\",\"Player ID\",\"Colour\",\"Role\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }

        }
        private bool insertIntoConfig(string type, string dateTime, string playerID, string colour, string role)
        {
            try
            {
                string line = $"\"{type}\",\"{dateTime}\",\"{playerID}\",\"{colour}\",\"{role}\"{Environment.NewLine}";
                File.AppendAllText("Configfile/report.csv", line);
                return true;
            }
            catch (Exception ee)
            {
                string temp = ee.Message;
                return false;
            }
        }
        #endregion
        #region Async and Communication Routine
        private static async Task RunPeriodicAsync(Action onTick,
                                           TimeSpan dueTime,
                                           TimeSpan interval,
                                           CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            // Repeat this loop until cancelled.
            while (!token.IsCancellationRequested)
            {
                // Call our onTick function.
                onTick?.Invoke();

                // Wait to repeat again.
                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, token);
            }
        }


        /* The `action` method will be called in the sep thread. */
        private static async Task RunAsync(Action action, TimeSpan dueTime, CancellationToken token)
        {
            // Initial wait time before we begin the periodic loop.
            if (dueTime > TimeSpan.Zero)
                await Task.Delay(dueTime, token);

            action?.Invoke();
            
        }
        private void CommunicationRoutine()
        {
            if (startgame)
            {  
                // Init Socket and register a game
                StartSocket();
                // Connect new players
                ConnectPlayers();
                // Notify players about the game
                BeginGame();
                startgame = false;
            }

            if (!endgame)
            {
                // Receive message from players while game is on
                // received messages will be passed to the method
                // AnalizeMessage
                Receive(AnalizeMessage);
            }

            // Update the board, strange behaviour :(
            UpdateBoard();

        }
        #endregion


        /* Analize received message from a player */
        private void AnalizeMessage(string obj)
        {
            dynamic magic = JsonConvert.DeserializeObject(obj);
            string action = magic.action;
            string playerId = magic.userGuid;
            switch (action.ToLower())
            {
                case "state":
                    {
                        // find player
                        Player player = findPlayerById(playerId);
                        if (player == null) return;
                        // get json to response
                        string json = GMRequestHandler.ResponseForDiscover(player);
                        // fill json
                        // TODO:
                        // response
                        Send(GMSocket, json);
                        // TODO: WRITE REPORT IN REPORT FILE
                        // Sammy, please check how to use one obj in multiple threads
                        // so we can write to the same file from different threads
                        break;
                    }
                case "move":
                    break; 
                // and so on ....
                default:
                    break;
            }
        }

    
        #region Add A New Piece During The Game
        private void addPiece()
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (true)
            {
                int row = rnd.Next(Board.GoalHeight, Board.GoalHeight + Board.TaskHeight);
                int column = rnd.Next(0, Board.Width);
                bool exist = false;
                if (RedTeam.isTaken(column, row) != 0) continue;
                if (BlueTeam.isTaken(column, row) != 0) continue;
                foreach (Piece p in board.Pieces)
                    if (p.isTaken(column, row))
                    {
                        exist = true;
                        break;
                    }
                if (exist) continue;
                Piece piece = new Piece();
                piece.row = row;
                piece.column = column;

                piece.isSham = (rnd.Next(0, 100) < Board.ShamProbability) ? true : false;
                board.Pieces.Add(piece);
                break;
            }

        }
        #endregion

        #region Player Routine and stuff
        private void doWork()
        {
            foreach (Player player in board.BlueTeam.members)
            {
                // prev coor
                playerRoutine(player);
                // write to file

            }

            foreach (Player player in board.RedTeam.members)
            {
                playerRoutine(player);
                // write to file
            }
        }
       

        
        private void playerRoutine(Player player)
        {
            PlayerDiscoversNeighboringCells(player);

            //If the player steps in the undiscovered red goal cell and he has a piece
            if (player.hasPiece() && player.Neighbors[1, 1] == Player.NeighborStatus.GOAL_AREA)
            {
                placesPiece(player);
                string Pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string Pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("PlacePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, Pc, Pr);
                return;
            }
            // if player.tochec is true thatn goRnd will check piece for being sham
            // if playwe.tochec is false thatn goRnd wil move a player
            player.goRnd();
            string pcc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string prr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("Move", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pcc, prr);



            int c = player.column;
            int r = player.row;

            if (!player.hasPiece())
            {
                takePiece(player);
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("TakePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                return;
            }


        }

        /* PLayer places a piece */
        private void placesPiece(Player player)
        {
            List<Goal> UndiscoveredGoals = (player.Team == Team.TeamColor.RED) ? board.UndiscoveredRedGoals : board.UndiscoveredBlueGoals;
            List<Goal> DiscoveredGoals = (player.Team == Team.TeamColor.RED) ? board.DiscoveredRedGoals : board.DiscoveredBlueGoals;

            /* Discover a goal */
            foreach (Goal goalDiscoverdByPlayer in UndiscoveredGoals)
            {
                if (goalDiscoverdByPlayer.row == player.row && goalDiscoverdByPlayer.column == player.column)
                {
                    UndiscoveredGoals.Remove(goalDiscoverdByPlayer); //Since goal has been discoverd, remove it.
                    DiscoveredGoals.Add(goalDiscoverdByPlayer); //Add "YG" to that (goal has been discoverd).
                    player.Piece = null; //Player no longer has the piece.

                    if (player.Team == Team.TeamColor.RED)
                        Board.RedScore++;
                    else
                        Board.BlueScore++;

                    if (player.Team == Team.TeamColor.BLUE)
                        BlueTeam.DiscoveredGoals.Add(new Goal { row = player.row, column = player.column });
                    else
                        RedTeam.DiscoveredGoals.Add(new Goal { row = player.row, column = player.column });

                    return;
                }
            }

            /* Discover a non-goal */
            if (player.Team == Team.TeamColor.BLUE)
                BlueTeam.DiscoveredNonGoals.Add(new Goal { row = player.row, column = player.column });
            else
                RedTeam.DiscoveredNonGoals.Add(new Goal { row = player.row, column = player.column });

            player.Piece = null; //Player no longer has the piece.
            checkVictory(player);
        }

        /* Player discovers its 8 neighbors */
        private void PlayerDiscoversNeighboringCells(Player player)
        {
            for (int c = 0; c < 3; c++)
                for (int r = 0; r < 3; r++)
                    player.Neighbors[c, r] = board.GetPlayersNeighbor(player.column - 1 + c, player.row - 1 + r, player.Team);

        }

        /** Player takes a  Piece **/
        private void takePiece(Player player)
        {
            int c = player.column;
            int r = player.row;

            foreach (Piece p in board.Pieces)
            {
                if (p.row == r && p.column == c)
                {
                    player.Piece = p;
                    board.Pieces.Remove(p);
                    break;
                }
            }

            player.checkPiece();
            // write to file
            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("TestPiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
        }
        #endregion

        #region Init GOALS and load BOARD, PIECES, TEAMS
        /**Undiscovered Goals are initialised randomly**/
        private void initGoals()
        {

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < Board.NumberOfGoals; i++)
            {
                Goal redgoal = new Goal();
                redgoal.row = rnd.Next(0, Board.GoalHeight);
                redgoal.column = rnd.Next(0, Board.Width);
                board.UndiscoveredRedGoals.Add(redgoal);
                board.boardtable[redgoal.column, redgoal.row] = Board.Status.UNDISCOVERED_GOAL;
            
                Goal bluegoal = new Goal();
                bluegoal.row = Board.Height - 1 - redgoal.row; ;
                bluegoal.column = redgoal.column;
                board.UndiscoveredBlueGoals.Add(bluegoal);
                board.boardtable[bluegoal.column, bluegoal.row] = Board.Status.UNDISCOVERED_GOAL;
            }
        }

        private void loadBoard()
        {

            string config = "Configfile/config";
            string json = "";
            if (!File.Exists(config))
            {
                MessageBox.Show("Config File does not exisrt");
                this.Close();
            }
            else
            {
                json = File.ReadAllText(config, Encoding.ASCII);
            }

            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            Board.RedScore = 0;
            Board.BlueScore = 0;
            Board.Width = magic.Width;
            Board.GoalHeight = magic.GoalHeight;
            Board.TaskHeight = magic.TaskHeight;
            Board.Height = 2 * Board.GoalHeight + Board.TaskHeight;
            Board.MaxNumOfPlayers = magic.MaxNumOfPlayers;
            Board.InitialNumberOfPieces = magic.InitialNumberOfPieces;
            Board.NumberOfGoals = magic.NumberOfGoals;
            Board.ShamProbability = magic.ShamProbability; // 50%

            board.boardtable = new Board.Status[Board.Width, Board.Height];
            for (int r = 0; r < Board.GoalHeight; r++)
                for (int c = 0; c < Board.Width; c++)
                {
                    board.boardtable[c, r] = Board.Status.RED_GOALS_CELL;
                    board.boardtable[c, Board.Height - 1 - r] = Board.Status.BLUE_GOALS_CELL;
                }

            RedTeam = loadTeam(Team.TeamColor.RED);
            board.RedTeam = RedTeam;

            BlueTeam = loadTeam(Team.TeamColor.BLUE);
            board.BlueTeam = BlueTeam;

            //Init list of goals
            board.UndiscoveredRedGoals = new List<Goal>();
            board.UndiscoveredBlueGoals = new List<Goal>();
            board.DiscoveredRedGoals = new List<Goal>();
            board.DiscoveredBlueGoals = new List<Goal>();
            board.NonGoals = new List<Goal>();

            board.Pieces = loadPieces();

            #region Grid rows and columns 
            /* Set up grounds rows */
            for (int row = 0; row < Board.Height; row++)
            {
                RowDefinition rowdef = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };
                playgroundDockPanel.RowDefinitions.Add(rowdef);
            }

            /* Set up grounds columns */
            for (int col = 0; col < Board.Width; col++)
            {
                ColumnDefinition coldef = new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };

                playgroundDockPanel.ColumnDefinitions.Add(coldef);
            }

            /* Column to place notes about the game */
            ColumnDefinition notes = new ColumnDefinition
            {
                Width = new GridLength(2, GridUnitType.Star)
            };
            playgroundDockPanel.ColumnDefinitions.Add(notes);
            #endregion

            initGoals();


        }

        private List<Piece> loadPieces()
        {
            List<Piece> pieces = new List<Piece>();

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            while (pieces.Count < Board.InitialNumberOfPieces)
            {
                int row = rnd.Next(Board.GoalHeight, Board.GoalHeight + Board.TaskHeight);
                int column = rnd.Next(0, Board.Width);
                bool exist = false;
                if (RedTeam.isTaken(column, row) != 0) continue;
                if (BlueTeam.isTaken(column, row) != 0) continue;
                foreach (Piece p in pieces)
                    if (p.isTaken(column, row))
                    {
                        exist = true;
                        break;
                    }
                if (exist) continue;
                Piece piece = new Piece();
                piece.row = row;
                piece.column = column;

                piece.isSham = (rnd.Next(0, 100) < Board.ShamProbability) ? true : false;
                pieces.Add(piece);
                board.boardtable[piece.column, piece.row] =
                    piece.isSham ? Board.Status.SHAM : Board.Status.PIECE;
            }
            return pieces;

        }

        private Team loadTeam(Team.TeamColor teamColor)
        {
            Team team = new Team();

            team.members = new List<Player>();
            team.DiscoveredGoals = new List<Goal>();
            team.DiscoveredNonGoals = new List<Goal>();
            team.teamColor = teamColor;

            return team;
        }
        #endregion

        private void UpdateBoard()
        {

            for (int row = 0; row < Board.Height; row++)
                for (int col = 0; col < Board.Width; col++)
                {
                    Image img = new Image
                    {
                        Visibility = Visibility.Visible,
                        Tag = "" + col + "x" + row,
                        Margin = new Thickness(2)
                    };

                    switch (board.boardtable[col, row]) 
                  //switch (board.getCellStatus(col, row))
                    {
                        case Board.Status.BLUE_GOALS_CELL:
                        case Board.Status.RED_GOALS_CELL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative));
                            break;

                        case Board.Status.RED_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player.png", UriKind.Relative));
                            break;

                        case Board.Status.BLUE_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player.png", UriKind.Relative));
                            break;

                        case Board.Status.RED_PLAYER_PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player_piece.png", UriKind.Relative));
                            break;

                        case Board.Status.BLUE_PLAYER_PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player_piece.png", UriKind.Relative));
                            break;



                        case Board.Status.TASK_CELL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/white_picture.png", UriKind.Relative));
                            break;

                        case Board.Status.PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/piece_picture.png", UriKind.Relative));
                            break;
                        case Board.Status.SHAM:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/sham_picture.png", UriKind.Relative));
                            break;

                        case Board.Status.UNDISCOVERED_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/undiscovered_goal.png", UriKind.Relative));
                            break;

                        case Board.Status.DISCOVERED_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/discovered_goal.png", UriKind.Relative));
                            break;

                        case Board.Status.DISCOVERED_NON_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/discovered_non_goal.png", UriKind.Relative));
                            break;

                    }
                    img.Stretch = Stretch.Fill;

                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);

                    playgroundDockPanel.Children.Add(img);
                }

            // please keep it as separate loops for now
            for (int row = 0; row < Board.Height; row++)
            {
                string line = "";
                for (int col = 0; col < Board.Width; col++)
                {
                    switch (board.boardtable[col, row])
                    {
                        case Board.Status.TASK_CELL: line += "TC"; break;
                        case Board.Status.PIECE: line += "PC"; break;
                        case Board.Status.SHAM: line += "SH"; break;
                        case Board.Status.RED_GOALS_CELL: line += "RG"; break;
                        case Board.Status.BLUE_GOALS_CELL: line += "BG"; break;
                        case Board.Status.RED_PLAYER: line += "RE"; break;
                        case Board.Status.BLUE_PLAYER: line += "BL"; break;
                        case Board.Status.UNDISCOVERED_GOAL: line += "UG"; break;
                        case Board.Status.DISCOVERED_GOAL: line += "DG"; break;
                        case Board.Status.DISCOVERED_NON_GOAL: line += "NG"; break;
                        case Board.Status.RED_PLAYER_PIECE: line += "RP"; break;
                        case Board.Status.BLUE_PLAYER_PIECE: line += "BP"; break;
                    }
                    line += " ";
                }
                Console.WriteLine(line);
            }
        }

        /* Socket Code, basically what we had in GMSocket class but 
         * BUT! please be careful with them, try to avoid changing */
        #region Socket Code

        #region Start Client and Connect 
        public void StartClient()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Loopback;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                GMSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                GMSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
                GMSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 10));

                GMSocket.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), GMSocket);
                connectDone.WaitOne();
                connectDone.Reset();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}\n",
                    client.RemoteEndPoint.ToString());

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        public string Receive(Action<string> cb = null)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = GMSocket;
                if (cb != null)
                    state.cb = cb;

                GMSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                receiveDone.WaitOne();
                receiveDone.Reset();
                var content = state.sb.ToString();
                content = content.Remove(content.IndexOf(ETB));
                state.sb.Clear();
                return content;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                if (bytesRead > 0)
                {
                    var content = state.sb.ToString();
                    if (content.IndexOf(ETB) > -1)
                    {
                        content = content.Remove(content.IndexOf(ETB));
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}\n",
                            content.Length, content);
                        //   RequestHandler.handleRequest(content, client);
                        //   state.sb.Clear();
                        receiveDone.Set();


                        if (state.cb != null)
                        {
                            state.cb(content);
                            state.cb = null;
                        }
                        //Receive();
                    }
                    else
                    {
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data + ETB);
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
           
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.\n", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region Find Player By ID
        /* Find player by id, no hash applied yet */
        private Player findPlayerById(string playerId)
        {
            foreach (Player p in RedTeam.members)
                if (p.playerID.Equals(playerId))
                    return p;
            foreach (Player p in BlueTeam.members)
                if (p.playerID.Equals(playerId))
                    return p;
            return null;
        }
        #endregion

        #region Check for Victory
        private void checkVictory(Player player)
        {
            if (board.DiscoveredBlueGoals.Count >= Board.NumberOfGoals)
            {
                // Blue WINS
                endgame = true;
                string message = "Congratulations, Blue team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                //int pId = Player.playerID;

                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);

                MessageBox.Show(message);
                this.Close();
            }
            if (board.DiscoveredRedGoals.Count >= Board.NumberOfGoals)
            {
                // Red WINS
                endgame = true;
                string message = "Congratulations, Red team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                MessageBox.Show(message);
                this.Close();
            }
        }
        #endregion


        /* not used yet, will be used for port and ip address */
        #region Command Line Parameters
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args != null && args.Count() > 0)
            {
                foreach (var arg in args)
                {
                    // write code to use the command line arg value 
                }
            }
        }
        #endregion

        
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public Action<string> cb = null;
    }
}
