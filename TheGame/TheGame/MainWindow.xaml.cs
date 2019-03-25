using System;
using System.Collections.Generic;
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

namespace TheGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Board board;
        private Team RedTeam;
        private Team BlueTeam;

        private bool pause;

        public MainWindow()
        {
            InitializeComponent();

            RedTeam = BlueTeam = null;
            pause = false;
            board = new Board();


            loadBoard();
            initGoals();
            loadPieces();
           
            updateBoard();

            var dueTime = TimeSpan.FromSeconds(5);
            var interval = TimeSpan.FromSeconds(1);

            // Add a CancellationTokenSource and supply the token here instead of None.
            RunPeriodicAsync(OnTick, dueTime, interval, CancellationToken.None);
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
            if (counter_tmp % 2 == 0 )
                addPiece();
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
                playerRoutine(player);
            }

            foreach (Player player in board.RedTeam.members)
            {
                playerRoutine(player);
            }
        }
        #endregion

        private void playerRoutine(Player player)
        {
            PlayerDiscoversNeighboringCells(player);

            //If the player steps in the undiscovered red goal cell and he has a piece
            if (player.hasPiece() && player.Neighbors[1, 1] == Player.NeighborStatus.GOAL_AREA)
            {
                placesPiece(player);
                return;
            }

            player.goRnd();

            int c = player.column;
            int r = player.row;

            if (!player.hasPiece())
            {
                takePiece(player);
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
                        BlueTeam.DiscoveredGoals.Add(new Goal { row = player.row, column = player.column});
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
        }

        /* Player discovers its 8 neighbors */
        private void PlayerDiscoversNeighboringCells(Player player)
        {
            for (int c = 0; c < 3; c++)
                for (int r = 0; r < 3; r++)
                    player.Neighbors[c, r] = board.GetPlayersNeighbor(player.column-1+c, player.row-1+r, player.Team);

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
            
        }

        /**Undiscovered Goals are initialised randomly**/
        private void initGoals() {

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
            Board.RedScore = 0;
            Board.BlueScore = 0;
            Board.Width = 6;   
            Board.GoalHeight = 3;
            Board.TaskHeight = 4;
            Board.Height = 2* Board.GoalHeight + Board.TaskHeight;
            board.InitialNumberOfPieces = 10;
            board.NumberOfGoals = 2;
            board.ShamProbability = 50; // 50%

            RedTeam = loadRedTeam();
            board.RedTeam = RedTeam;

            BlueTeam = loadBlueTeam();
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

        }

        #region Pieces
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

        #region Teams
        private Team loadBlueTeam()
        {
            Team team = new Team();
            team.leader = new Player
            {
                role = Player.Role.LEADER,
                playerID = 10,
                row = 6,
                column = 1,
                Team = Team.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3]
            };

            team.members = new List<Player>();
            team.DiscoveredGoals = new List<Goal>();
            team.DiscoveredNonGoals = new List<Goal>();

            team.members.Add(new Player
            {
                role = Player.Role.MEMBER,
                playerID = 12,
                row = 7,
                column = 3,
                Team = Team.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3]
            });
            team.members.Add(team.leader);
            team.teamColor = Team.TeamColor.BLUE;

            team.maxNumOfPlayers = 2;

            return team;
        }

        private Team loadRedTeam()
        {
            Team team = new Team();
            team.leader = new Player
            {
                role = Player.Role.LEADER,
                playerID = 10,
                row = 1,
                column = 1,
                Team = Team.TeamColor.RED,
                Neighbors = new Player.NeighborStatus[3,3]
            };

            team.members = new List<Player>();
            team.DiscoveredGoals = new List<Goal>();
            team.DiscoveredNonGoals = new List<Goal>();

            team.members.Add(new Player
            {
                role = Player.Role.MEMBER,
                playerID = 12,
                row = 1,
                column = 3,
                Team = Team.TeamColor.RED,
                Neighbors = new Player.NeighborStatus[3, 3]
            });
            team.members.Add(team.leader);
            team.teamColor = Team.TeamColor.RED;

            team.maxNumOfPlayers = 2;

            return team;
        }
        #endregion

     
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

    }
}
