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

namespace TheGame
{
    public partial class MainWindow : Window
    {
        private Board board;
        private Team RedTeam;
        private Team BlueTeam;
        private GMSocket GMSocket;
        private bool pause;

        
        public MainWindow()
        {

            InitializeComponent();
            ConsoleWriteLine("Game master has started.");
            RedTeam = BlueTeam = null;
            pause = false;
            board = new Board();

            initFile();
            loadBoard();

            updateBoard();

            var dueTime = TimeSpan.FromSeconds(5);
            var interval = TimeSpan.FromSeconds(1);

            // Add a CancellationTokenSource and supply the token here instead of None.
 //           RunPeriodicAsync(OnTick, dueTime, interval, CancellationToken.None);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartSocket();
            ConnectPlayers();
            BeginGame();
            LetsMove();

            updateBoard();
        }

        private void StartSocket()
        {
            ConsoleWriteLine("GM Socket started.");

            GMSocket = new GMSocket();
            GMSocket.StartClient();
            GMRequestHandler.SendSetUpGame(GMSocket);
            GMRequestHandler.allDone.WaitOne();

            dynamic magic = 
                JsonConvert.DeserializeObject(GMRequestHandler.Response);
            string action = magic.action;
            string result = magic.result;
            ConsoleWriteLine("action: "+action);
            ConsoleWriteLine("result: "+result);
            if (result.ToLower().Equals("denied"))
            {
                ConsoleWriteLine("Game has been denied");
                MessageBox.Show("Game has been denied",
                    "Error");
                this.Close();
            }
        }
        private void ConnectPlayers()
        {
            if (null == GMSocket)
            {
                ConsoleWriteLine("Socket is null");
                MessageBox.Show("Socket is null", "Error");
                this.Close();
            }

            ConsoleWriteLine("Expect players");
            while(board.RedTeam.NumOfPlayers  < Board.MaxNumOfPlayers || 
                  board.BlueTeam.NumOfPlayers < Board.MaxNumOfPlayers)
            {
                Player player;
                GMRequestHandler.ConnectPlayer(GMSocket, out player);
                if (player.Team == Team.TeamColor.RED)
                {
                    if (board.RedTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                        GMRequestHandler.ConnectPlayerDeny(GMSocket, player);
                    if (board.RedTeam.leader == null)
                        board.RedTeam.leader = player;
                    player.Neighbors = new Player.NeighborStatus[3, 3];
                    player.row = 1; // TODO: Update the row
                    player.column = RedTeam.members.Count;
                    board.RedTeam.members.Add(player);
                }
                else
                {
                    if (board.BlueTeam.NumOfPlayers == Board.MaxNumOfPlayers)
                        GMRequestHandler.ConnectPlayerDeny(GMSocket, player);
                    if (board.BlueTeam.leader == null)
                        board.BlueTeam.leader = player;
                    player.Neighbors = new Player.NeighborStatus[3, 3];
                    player.row = Board.Height - 1;  // TODO: Update the row
                    player.column = BlueTeam.members.Count;
                    board.BlueTeam.members.Add(player);
                }
                GMRequestHandler.ConnectPlayerOK(GMSocket, player);
                ConsoleWriteLine("Players connected "+RedTeam.NumOfPlayers+"|"+
                    BlueTeam.NumOfPlayers+"|"+Board.MaxNumOfPlayers);
            }
            ConsoleWriteLine("All Players Connected");

        }
        private void BeginGame()
        {
            ConsoleWriteLine("");
            ConsoleWriteLine("Begin the game for Blue Team");
            foreach (var p in BlueTeam.members)
            {
                GMRequestHandler.BeginGame(GMSocket, p, BlueTeam.members, BlueTeam.leader);
            }
            ConsoleWriteLine("Begin the game for Red Team");
            foreach (var p in RedTeam.members)
            {
                GMRequestHandler.BeginGame(GMSocket, p, RedTeam.members, RedTeam.leader);
            }
            ConsoleWriteLine("Started");
        }

        private void LetsMove()
        {
            ConsoleWriteLine("");
            ConsoleWriteLine("Move player from Blue Team");
            foreach (var p in BlueTeam.members)
            {
                GMRequestHandler.ResponseForMove(GMSocket, p);
            }
            ConsoleWriteLine("Move player from Red Team");
            foreach (var p in RedTeam.members)
            {
                GMRequestHandler.ResponseForMove(GMSocket, p);
            }

        }

        
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

        #region Async Cyclic Method
        // The `onTick` method will be called periodically unless cancelled.
        private static async Task RunPeriodicAsync(Action onTick, TimeSpan dueTime, TimeSpan interval, CancellationToken token)
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

        private int counter_tmp = 0;
        private void OnTick()
        {
            if (pause) return;
            doWork();
            updateBoard();
            counter_tmp++;
            if (counter_tmp % 2 == 0 && board.Pieces.Count < board.InitialNumberOfPieces)
                addPiece();


        }

        private void checkVictory(Player player)
        {
            if (board.DiscoveredBlueGoals.Count >= board.NumberOfGoals)
            {
                // Blue WINS
                pause = true;
                string message = "Congratulations, Blue team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                //int pId = Player.playerID;

                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);

                MessageBox.Show(message);
                this.Close();
            }
            if (board.DiscoveredRedGoals.Count >= board.NumberOfGoals)
            {
                // Red WINS
                pause = true;
                string message = "Congratulations, Red team wins!";
                string pc = (player.Team == Team.TeamColor.RED) ? "red" : "blue";
                string pr = (player.role == Player.Role.LEADER) ? "leader" : "member";
                insertIntoConfig("Victory", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                insertIntoConfig("Defeat", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), player.playerID, pc, pr);
                MessageBox.Show(message);
                this.Close();
            }
        }

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

                piece.isSham = (rnd.Next(0, 100) < board.ShamProbability) ? true : false;
                board.Pieces.Add(piece);
                break;
            }

        }

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
        #endregion

        #region Player Routine and stuff
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

        /**Undiscovered Goals are initialised randomly**/
        private void initGoals()
        {

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < board.NumberOfGoals; i++)
            {
                Goal goal = new Goal();
                goal.row = rnd.Next(0, Board.GoalHeight);
                goal.column = rnd.Next(0, Board.Width);
                board.UndiscoveredRedGoals.Add(goal);
            }

            for (int i = 0; i < board.NumberOfGoals; i++)
            {
                Goal goal = new Goal();
                goal.row = rnd.Next(Board.Height - Board.GoalHeight, Board.Height);
                goal.column = rnd.Next(0, Board.Width);
                board.UndiscoveredBlueGoals.Add(goal);
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
            board.InitialNumberOfPieces = magic.InitialNumberOfPieces;
            board.NumberOfGoals = magic.NumberOfGoals;
            board.ShamProbability = magic.ShamProbability; // 50%

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
            loadPieces();

        }

        private void updateBoard()
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

                    switch (board.getCellStatus(col, row))
                    {
                        case (int)Board.Status.BLUE_GOAL_AREA:
                        case (int)Board.Status.RED_GOAL_AREA:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.RED_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.BLUE_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.RED_PLAYER_PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player_piece.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.BLUE_PLAYER_PIECE:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player_piece.png", UriKind.Relative));
                            break;



                        case (int)Board.Status.TASK_AREA:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/white_picture.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.PIECE_AREA:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/piece_picture.png", UriKind.Relative));
                            break;
                        case (int)Board.Status.SHAM_AREA:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/sham_picture.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.UNDISCOVERED_GOALS:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/undiscovered_goal.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.DISCOVERED_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/discovered_goal.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.DISCOVERED_NON_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/discovered_non_goal.png", UriKind.Relative));
                            break;

                    }
                    img.Stretch = Stretch.Fill;

                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);

                    playgroundDockPanel.Children.Add(img);
                }
        }

        private void pauseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            pause = !pause;
        }


        #region Load Pieces
        private List<Piece> loadPieces()
        {
            List<Piece> pieces = new List<Piece>();

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            while (pieces.Count < board.InitialNumberOfPieces)
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

                piece.isSham = (rnd.Next(0, 100) < board.ShamProbability) ? true : false;
                pieces.Add(piece);
            }
            return pieces;

        }
        #endregion

        #region Load Team 
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

        #region WPF Console writting
        public void ConsoleWrite(string line)
        {
            string curr = this.ConsoleTextBlock.Text;
            curr += "> " + line;
            this.ConsoleTextBlock.Text = curr;
        }
        public void ConsoleWriteLine(string line)
        {
            ConsoleWrite(line + "\n");
        }
        #endregion

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
}
