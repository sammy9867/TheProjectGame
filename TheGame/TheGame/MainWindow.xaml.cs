﻿using Newtonsoft.Json;
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
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace TheGame
{
    public partial class MainWindow : Window
    {
        private Board board;
        private Team RedTeam;
        private Team BlueTeam;

        private const char ETB = (char)23;
        string IP_ADDRESS = "";
        string PORT = "";

        //ManualResetEvent instances signal completion.
        public ManualResetEvent connectDone =
            new ManualResetEvent(false);
        public ManualResetEvent sendDone =
            new ManualResetEvent(false);
        public ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        public Socket GMSocket;

        //Boolean to indicate if the game is over
        private bool endgame;
        private bool startgame;

        //Timestamp
        long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        public MainWindow()
        {

            InitializeComponent();
            Console.WriteLine("Game master has started.");

            String[] args = App.Args;

            if (args.Length == 2)
            {
                try
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        Console.WriteLine("" + i + " : " + args[i]);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine("\n");
                    Console.WriteLine(e.Message);
                }

                IP_ADDRESS = args[0];
                PORT = args[1];
            }
            Console.WriteLine("IP_ADDRESS :    " + IP_ADDRESS);
            Console.WriteLine("PORT :    " + PORT);

            RedTeam = BlueTeam = null;
            endgame = false;
            startgame = true;

            board = new Board();

            initFile();
            initFilejSON();
            loadBoard();

            UpdateBoard();

            // offset before running communication routine
            var dueTime = TimeSpan.FromSeconds(1);

            // run separate thread for communication routine
            //            RunAsync(CommunicationRoutine, dueTime, CancellationToken.None);

            var interval = TimeSpan.FromMilliseconds(10); // FromSeconds(0); //TimeSpan.FromMilliseconds(0.1)
            RunPeriodicAsync(CommunicationRoutine, dueTime, interval, CancellationToken.None);
            RunPeriodicAsync(AddPiece, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(Board.FrequencyOfPlacingNewPiece), CancellationToken.None);
        }


        #region Start Socket
        /* Start Socket */
        private void StartSocket()
        {
            Console.WriteLine("GM Socket started.");
            // Start client sync
            StartClient(IP_ADDRESS, Convert.ToInt32(PORT));

            // Sending SetUp reuest and getting response
            Console.WriteLine("Sending SetUp reuest and getting response.");
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
        /* Method to connect players, while there is at least one available slot */
        private void ConnectPlayers()
        {
            if (null == GMSocket)
            {
                Console.WriteLine("Socket is null");
                MessageBox.Show("Socket is null", "Error");
                this.Close();
            }

            Console.WriteLine("Expect players");
            while(board.RedTeam.NumOfPlayers < Board.MaxNumOfPlayers ||
                  board.BlueTeam.NumOfPlayers < Board.MaxNumOfPlayers)
            {
                // Received Message is analized by ConnectPlayer method
                Receive(ConnectPlayer);

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

            if (player.Team == Team.TeamColor.RED)
            {
                if (board.RedTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                {
                    Send(GMSocket, GMRequestHandler.ConnectPlayerDeny(player));
                    return;
                }
                if (board.RedTeam.leader == null)
                {
                    board.RedTeam.leader = player;
                    player.role = Player.Role.LEADER;
                }
                else
                {
                    player.role = Player.Role.MEMBER;
                }

                player.Row = Board.Height - 1; // TODO: Update the row
                player.Column = RedTeam.members.Count;
                board.RedTeam.members.Add(player);
                board.boardtable[player.Column, player.Row] = Board.Status.RED_PLAYER;
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("Connect", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
            }
            else
            {
                if (board.BlueTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                {
                    Send(GMSocket, GMRequestHandler.ConnectPlayerDeny(player));
                    return;
                }

                if (board.BlueTeam.leader == null)
                {
                    board.BlueTeam.leader = player;
                    player.role = Player.Role.LEADER;
                }
                else
                {
                    player.role = Player.Role.MEMBER;
                }

                player.Row = 1;  // TODO: Update the row
                player.Column = BlueTeam.members.Count;
                board.BlueTeam.members.Add(player);
                board.boardtable[player.Column, player.Row] = Board.Status.BLUE_PLAYER;
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("Connect", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
            }


            Send(GMSocket, GMRequestHandler.ConnectPlayerOK(player));

            if(board.RedTeam.NumOfPlayers == Board.MaxNumOfPlayers && 
                  board.BlueTeam.NumOfPlayers == Board.MaxNumOfPlayers )
                connectDone.Set();
        }
        #endregion

        #region Begin Game
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
        private void initFile()
        {
            // create a file object
            // create file itself if it does not exist
            string newFileName = @"..\..\Configfile\reportlog.csv";
            

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
                File.AppendAllText(@"..\..\Configfile\JSONLogo.txt", line);
                return true;
            }
            catch (Exception ee)
            {
                string temp = ee.Message;
                return false;
            }
        }

        private void initFilejSON()
        {
            // create a file object
            // create file itself if it does not exist
            string newFileName = @"..\..\Configfile\JSONLog.txt";


            if (!File.Exists(newFileName))
            {
                string clientHeader = $"\"Message\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }
            else if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
                string clientHeader = $"\"Message\"{Environment.NewLine}";
                File.WriteAllText(newFileName, clientHeader);
            }

        }

        private bool insertIntoConfigJSON(string m)
        {
            try
            {
                string line = $"\"{m}\"{Environment.NewLine}";
                File.AppendAllText(@"..\..\Configfile\JSONLogo.txt", line);
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
                connectDone.WaitOne();  // wait untill ConnectPlayer completes
                connectDone.Reset();    // reset handler

                Console.WriteLine("All players connected.... One sec to take a nap");
                Thread.Sleep(1000);
                Console.WriteLine("Oh, here we go again");

                // Notify players about the game
                BeginGame();
                startgame = false;

            }

            if (!endgame)
            {
                // Receive message from players while game is on
                // received messages will be passed to the method
                // AnalizeMessage
                Receive(AnalyzeMessage);
            }

            // Update the board, strange behaviour :(
            Application.Current.Dispatcher.Invoke((Action)delegate {
                UpdateBoard();
            });

        }
        #endregion

        #region Analyze Received Message
        /* Analyze received message from a player */
        private void AnalyzeMessage(string obj)
        {
            if (obj == null || obj == "") return;

            dynamic magic = JsonConvert.DeserializeObject(obj);
            string action = magic.action;
            if (action.ToLower().Equals("connect"))
            {
                ConnectPlayer(obj);
                return;
            }

            string playerId = magic.userGuid;
            Player player;
            // find player
            player = FindPlayerById(playerId);
            if (player == null) return;

            switch (action.ToLower())
            {
                case "state":
                    {

                        // get json to response
                        string json = GMRequestHandler.ResponseForDiscover(player);
                        // fill json
                        PlayerDiscoversNeighboringCells(player, ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.DiscoveryDelay);
                        Send(GMSocket, json);
                        break;
                    }
                case "move":
                    {
                        // get json to response
                        string json = GMRequestHandler.ResponseForMove(player);
                        // fill json
                        PlayerMove(player,(string) magic.direction,  ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.MoveDelay);
                        Send(GMSocket, json);
                        Console.WriteLine(json);
                        break;
                    }
                case "pickup":
                    {
                        // get json to response
                        string json = GMRequestHandler.ResponseForPickUp(player);
                        // fill json
                        PlayerPickupPiece(player, ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.PickUpDelay);
                        Send(GMSocket, json);
                        break;
                    }
                case "test":
                    {
                        // get json to response
                        string json = GMRequestHandler.ResponseForTestPiece(player); 
                        // fill json
                        PlayerTestPiece(player, ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.TestDelay);
                        Send(GMSocket, json);
                        break;
                    }
                case "destroy":
                    {
                        // get json to response
                        string json = GMRequestHandler.ResponseForDestroyPiece(player);
                        // fill json
                        PlayerDestroyPiece(player, ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.DestroyDelay);
                        Send(GMSocket, json);
                        break;
                    }
                case "place":
                    {
                        // get json to response
                        string json = GMRequestHandler.ResponseForPlacePiece(player); 
                        // fill json
                        PlayerPlacesPiece(player, ref json);
                        insertIntoConfigJSON(json);
                        // response
                        Thread.Sleep(Board.PlaceDelay);
                        Send(GMSocket, json);
                        break;
                    }
                case "send":
                case "exchange":
                    {
                        // find player
                        Player receiver = FindPlayerById((string)magic.receiverGuid);
                        if (receiver == null) return;
                        Thread.Sleep(Board.ExchangeDelay);
                        Send(GMSocket, Newtonsoft.Json.JsonConvert.SerializeObject(magic));
                        break;
                    }
                default:
                    break;
            }
            Console.WriteLine(action + " -->  " + player.playerID);
        }

        #endregion

        #region Add A New Piece During The Game
        private void AddPiece()
        {
            if (board.Pieces.Count >= Board.InitialNumberOfPieces)
                return;

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

        #region Player Routine
        /* Player Move */
        public void PlayerMove(Player player, string direction, ref string json)
        {
            // JObject jobject = JObject.Parse(json);
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            bool denied = false;
            Console.WriteLine(player.playerID + "["+player.Team+"] goes "+direction);
            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            switch (direction)
            {
                case "N":
                    if ((board.getCellStatus(player.X, player.Y + 1) & Board.Status.BLOCKED) != 0)
                    {
                        denied = true;
                        break;
                    }
                    player.goUp();
                   
                    insertIntoConfig("Move", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                    break;
                case "W":
                    if ((board.getCellStatus(player.X - 1, player.Y) & Board.Status.BLOCKED) != 0)
                    { // getCellStatus
                        denied = true;
                        break;
                    }
                    player.goLeft();
                    insertIntoConfig("Move", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                    break;
                case "E":
                    if ((board.getCellStatus(player.X + 1, player.Y) & Board.Status.BLOCKED) != 0)
                    { // getCellStatus
                        denied = true;
                        break;
                    }
                    player.goRight();
                    insertIntoConfig("Move", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                    break;
                case "S":
                    if ((board.getCellStatus(player.X , player.Y - 1) & Board.Status.BLOCKED) != 0)
                    { // getCellStatus
                        denied = true;
                        break;
                    }
                    player.goDown();
                    insertIntoConfig("Move", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                    break;
            }
            int md = Board.Height + Board.Width;
            foreach(Piece p in board.Pieces)
            {
                int dx = Math.Abs( p.column - player.Column );
                int dy = Math.Abs(p.row - player.Row);
                if (md > dx + dy)
                    md = dx + dy;
            }
            if (denied)
            {
                // getCellStatus
                magic.result = "denied";

                magic.manhattanDistance = null;
                json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
                return;
            }

            magic.manhattanDistance = md;

            json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
        }

        /* PLayer places a piece */
        public void PlayerPlacesPiece(Player player, ref string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            List<Goal> UndiscoveredGoals = (player.Team == Team.TeamColor.RED) ? board.UndiscoveredRedGoals : board.UndiscoveredBlueGoals;
            List<Goal> DiscoveredGoals = (player.Team == Team.TeamColor.RED) ? board.DiscoveredRedGoals : board.DiscoveredBlueGoals;

            /* Check if we place on already discoverd goal */
            foreach (Goal goalDiscoverdByPlayer in DiscoveredGoals)
            {
                if (goalDiscoverdByPlayer.row == player.Row && goalDiscoverdByPlayer.column == player.Column)
                {
                    magic.result = "denied";  // since it is OK by default
                    magic.consequence = null;

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
                    /* Player is not placing a piece, it keeps holding it */
                    return;
                }
            }
            /* Discover a goal */
            Console.WriteLine("BEFORE GETTING INTO UG");
            foreach (Goal goalDiscoverdByPlayer in UndiscoveredGoals)
            {
                Console.WriteLine("GOAL [" + goalDiscoverdByPlayer.row + ", " + goalDiscoverdByPlayer.column + "]");
                Console.WriteLine("Player [" + player.Row + ", " + player.Column + "]");

                if (goalDiscoverdByPlayer.row == player.Row && goalDiscoverdByPlayer.column == player.Column)
                {                 
                    UndiscoveredGoals.Remove(goalDiscoverdByPlayer); //Since goal has been discoverd, remove it.
                    DiscoveredGoals.Add(goalDiscoverdByPlayer); //Add "YG" to that (goal has been discoverd).

                    player.Piece = null; //Player no longer has the piece.

                    if (player.Team == Team.TeamColor.RED)
                        Board.RedScore++;
                    else
                        Board.BlueScore++;

                    if (player.Team == Team.TeamColor.BLUE)
                    {
                        BlueTeam.DiscoveredGoals.Add(new Goal { row = player.Row, column = player.Column });
                        string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                        insertIntoConfig("PlacePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "blue", pr);
                    }
                    else
                    {
                        RedTeam.DiscoveredGoals.Add(new Goal { row = player.Row, column = player.Column });
                        string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                        insertIntoConfig("PlacePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "red", pr);

                    }
                       

                    Console.WriteLine("Blue :" + Board.BlueScore);
                    Console.WriteLine("Red :" + Board.RedScore);
                    magic.consequence = "correct";

                    json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);

                    CheckVictory(player,ref json);

                    return;
                }
            }

            /* Discover a non-goal */
            if (player.Team == Team.TeamColor.BLUE)
            {
                BlueTeam.DiscoveredNonGoals.Add(new Goal { row = player.Row, column = player.Column });
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("PlacePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "blue", pr);
            }
            else
            {
                RedTeam.DiscoveredNonGoals.Add(new Goal { row = player.Row, column = player.Column });
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("PlacePiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "red", pr);

            }

            player.Piece = null; //Player no longer has the piece.

            magic.consequence = "meaningless";

            json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);

           
        }
        
        /*To check which team has won*/
        private void CheckVictory(Player player, ref string json)
        {
            Console.WriteLine("BLUE: " + Board.BlueScore + "RED:  " + Board.RedScore + "TOTAL: " + Board.NumberOfGoals);
            if (Board.BlueScore >= Board.NumberOfGoals)
            {
                // Blue WINS
                endgame = true;
                json = GMRequestHandler.SendGameOver(Team.TeamColor.BLUE);

                string message = "Congratulations, Blue team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                //int pId = Player.playerID;

                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "blue", pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "red", pr);
                Console.WriteLine(message);

            }
            if (Board.RedScore >= Board.NumberOfGoals)
            {
                // Red WINS
                endgame = true;
                json = GMRequestHandler.SendGameOver(Team.TeamColor.RED);

                string message = "Congratulations, Red team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "red", pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, "blue", pr); 
                Console.WriteLine(message);

            }
        }

        /* Player Discovers its 8 neighbors */
        public void PlayerDiscoversNeighboringCells(Player player, ref string json)
        {
            JObject magic = JObject.Parse(json);
            magic["userGuid"] = player.playerID;
            magic["result"] = "OK";
            JObject scope = (JObject) magic["location"];
            scope["x"] =player.X;
            scope["y"] = player.Y;
            List<JField> jfields = new List<JField>();

            for (int c = 0; c < 3; c++)
                for (int r = 2; r >= 0; r--)
                {
                    int column = player.Column - 1 + c;
                    int row = player.Row - 1 + r;
                    if (column < 0) continue;
                    if (column >= Board.Width) continue;
                    if (row < 0) continue;
                    if (row >= Board.Height) continue;
                    JField jField = new JField
                    {
                        x = column,
                        y = row
                    };
                    jField.value = new JFieldValue();
                    jField.value.manhattanDistance =
                        (Math.Abs(player.X - column) + Math.Abs(player.Y - row));

                    jField.value.userGuid = null;

                    switch (board.getCellStatus(column, row))
                    {
                        case Board.Status.TASK_CELL:
                        case Board.Status.RED_GOALS_CELL:
                        case Board.Status.BLUE_GOALS_CELL:
                        case Board.Status.UNDISCOVERED_GOAL:   // Player does not receive this data
                        case Board.Status.DISCOVERED_NON_GOAL: // Player gets it via Knowledge exchange
                            jField.value.contains = "empty";   // could be set as default
                            break;
                        case Board.Status.PIECE:
                        case Board.Status.SHAM:
                            jField.value.contains = "piece";
                            break;
                        case Board.Status.RED_PLAYER:
                        case Board.Status.BLUE_PLAYER:
                        case Board.Status.RED_PLAYER_WITH_PIECE:
                        case Board.Status.BLUE_PLAYER_WITH_PIECE:
                            jField.value.contains = "empty"; // ??? right?
                            jField.value.userGuid = FindPlayerByCoor(column, row).playerID;
                            break;
                        case Board.Status.DISCOVERED_GOAL:
                            jField.value.contains = "goal";
                            break;
                    }

                    jfields.Add(jField);
                }


            magic["fields"] = (JArray) JToken.FromObject(jfields);
            json = magic.ToString();

            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("Discover", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
        }

        /* Player takes a Piece */
        public void PlayerPickupPiece(Player player, ref string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            int c = player.Column;
            int r = player.Row;
            bool found = false;
            foreach (Piece p in board.Pieces)
            {
                if (p.row == r && p.column == c)
                {
                    player.Piece = p;
                    board.Pieces.Remove(p);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                magic.result = "denied";
            }
            json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
            // write to file
            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("PickupPiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
        }

        /* Player tests a Piece */
        public void PlayerTestPiece(Player player, ref string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            int c = player.Column;
            int r = player.Row;

            if (!player.hasPiece())
            {
                magic.result = "denied";
                magic.test = null;
                json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
                return;
            }
            if (player.Piece.isSham)
            {
                magic.result = "OK";
                magic.test = "true";
                json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
                return;
            }

            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("TestPiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
            // JSON's "test": "false" is default
            json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
 
            // write to file
   //         string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
     //       string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
         //   insertIntoConfig("TestPiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
        }

        /* Player destroys a Piece */
        public void PlayerDestroyPiece(Player player, ref string json)
        {
            dynamic magic = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            int c = player.Column;
            int r = player.Row;

            if (!player.hasPiece())
            {
                magic.result = "denied";
                json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
                return;
            }

            player.Piece = null;

            string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
            string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
            insertIntoConfig("DestroyPiece", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);

            json = Newtonsoft.Json.JsonConvert.SerializeObject(magic);
        }

        /*Time in milliseconds*/
        public long GetTimestamp()
        {
           long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
           return (now -  milliseconds);
        }

        #endregion

        #region Init GOALS and load BOARD, PIECES, TEAMS
        /*Undiscovered Goals are initialised randomly*/
        private void initGoals()
        {
            List<int> hBlueCol = new List<int>();

            for (int i = 0; i < Board.Width; i++)
            {
                hBlueCol.Add(i);     
            }
            Console.WriteLine();
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < Board.NumberOfGoals; i++)
            {
                Goal bluegoal = new Goal();


                bluegoal.row = rnd.Next(0, Board.GoalHeight);
                int r2 = rnd.Next(hBlueCol.Count);
                bluegoal.column = hBlueCol[r2];
                if (hBlueCol.Contains(bluegoal.column)) hBlueCol.Remove(bluegoal.column);


                board.UndiscoveredBlueGoals.Add(bluegoal);
                Console.WriteLine("BLUE :" + bluegoal.row + ", " + bluegoal.column);
                board.boardtable[bluegoal.column, bluegoal.row] = Board.Status.UNDISCOVERED_GOAL;

                Goal redgoal = new Goal();
                redgoal.row = Board.Height - 1 - bluegoal.row;
                redgoal.column = bluegoal.column;
                board.UndiscoveredRedGoals.Add(redgoal);
                Console.WriteLine("RED :" + redgoal.row + ", " + redgoal.column);
                board.boardtable[redgoal.column, redgoal.row] = Board.Status.UNDISCOVERED_GOAL;
           
            }
        }

        private void loadBoard()
        {

            string config = @"..\..\Configfile\config";
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
            Board.FrequencyOfPlacingNewPiece = magic.FrequencyOfPlacingNewPiece;
            Board.DiscoveryDelay = magic.DiscoveryDelay;
            Board.MoveDelay = magic.MoveDelay;
            Board.PickUpDelay = magic.PickUpDelay;
            Board.TestDelay = magic.TestDelay;
            Board.DestroyDelay = magic.DestroyDelay;
            Board.PlaceDelay = magic.PlaceDelay;
            Board.ExchangeDelay = magic.ExchangeDelay;

            Console.WriteLine("Initialization: ");
            Console.WriteLine(" Board.MaxNumOfPlayers: " + Board.MaxNumOfPlayers);
            Console.WriteLine(" Board.NumberOfGoals: " + Board.NumberOfGoals);
            Console.WriteLine(" Board.ShamProbability: " + Board.ShamProbability);
            Console.WriteLine(" Board.FrequencyOfPlacingNewPiece: " + Board.FrequencyOfPlacingNewPiece);

            board.boardtable = new Board.Status[Board.Width, Board.Height];
            for (int r = 0; r < Board.GoalHeight; r++)
                for (int c = 0; c < Board.Width; c++)
                {
                    board.boardtable[c, r] = Board.Status.BLUE_GOALS_CELL;
                    board.boardtable[c, Board.Height - 1 - r] = Board.Status.RED_GOALS_CELL;
                }

            RedTeam = new Team(Team.TeamColor.RED);
            board.RedTeam = RedTeam;

            BlueTeam = new Team(Team.TeamColor.BLUE);
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

        #endregion

        private void UpdateBoard()
        {

            for (int row = Board.Height - 1; row >= 0; row--)
                for (int col = 0; col < Board.Width; col++)
                {
                    Image img = new Image
                    {
                        Visibility = Visibility.Visible,
                        Tag = "" + col + "x" + row,
                        Margin = new Thickness(2)
                    };

                  switch (board.getCellStatus(col, row))
                    {
                        case Board.Status.RED_GOALS_CELL:
                        case Board.Status.BLUE_GOALS_CELL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative));
                            break;

                        case Board.Status.RED_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player.png", UriKind.Relative));
                            break;

                        case Board.Status.BLUE_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player.png", UriKind.Relative));
                            break;

                        case Board.Status.RED_PLAYER_WITH_PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player_piece.png", UriKind.Relative));
                            break;

                        case Board.Status.BLUE_PLAYER_WITH_PIECE:
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
                    Grid.SetRow(img, Board.Height - 1-  row);

                    playgroundDockPanel.Children.Add(img);
                }

        }

        /* Socket Code, basically what we had in GMSocket class*/
        #region Socket Code

        #region Start Client and Connect 
        public void StartClient(string ipAddress, int port)
        {
            try
            {
                Console.WriteLine("Game Master IP: " + ipAddress.ToString());
                Console.WriteLine("Game Master PORT: " + port);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);

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
                return null /*content*/;
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
                        Console.WriteLine("[GAME_MASTER] read:\n");

                        if (state.cb != null)
                        {
                            foreach (String _content in content.Split(ETB))
                            {
                                if (_content == null || _content == "") continue;
                                AnalyzeMessage(_content);
                            }
                            state.cb = null;
                        }
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
                receiveDone.Set();
            }
        }

        public void Send(Socket handler, String data)
        {
            // Remove useless white spaces 
            data = Regex.Replace(data, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
            // Add ETB at the end of the string
            data += ETB;
            Console.WriteLine("GAME_MASTER sent: \n"+data+"\n");
            // Create bytes 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            
            // Sending
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
           
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region Find Player By *
        /* Find player by id, no hash applied yet */
        private Player FindPlayerById(string playerId)
        {
            foreach (Player p in RedTeam.members)
                if (p.playerID.Equals(playerId))
                    return p;
            foreach (Player p in BlueTeam.members)
                if (p.playerID.Equals(playerId))
                    return p;

            Console.WriteLine("\n\n404 Player not found\n" +
                                playerId + "\n\n");
            return null;
        }
        private Player FindPlayerByCoor(int col, int row)
        {
            foreach (Player p in RedTeam.members)
                if (p.Row == row && p.Column == col)
                    return p;
            foreach (Player p in BlueTeam.members)
                if (p.Row ==row && p.Column == col)
                    return p;
            return null;
        }

        #endregion

        #region Command Line Parameters
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args != null && args.Count() > 0)
            {
                IP_ADDRESS = args[1];
                PORT = args[2];

                Console.WriteLine(IP_ADDRESS + " " + PORT);
            }
        }
        #endregion
        
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 2048;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public Action<string> cb = null;
    }
}
