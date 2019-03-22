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

        public MainWindow()
        {
            InitializeComponent();

            RedTeam = BlueTeam = null;

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
        private void OnTick()
        {
            doWork();
            updateBoard();
        }

        private void doWork()
        {
            int rm = RedTeam.leader.goRnd();
            int bm = BlueTeam.leader.goRnd();

        /*    if (rm == 7)
            {
                Goal goal = new Goal
                {
                    column = RedTeam.leader.column,
                    row = RedTeam.leader.row
                };
                board.Goals.Add(goal);
            }
            if (bm == 7)
            {
                Goal goal = new Goal
                {
                    column = BlueTeam.leader.column,
                    row = BlueTeam.leader.row
                };
                board.Goals.Add(goal);
            }*/


            int c = RedTeam.leader.column;
            int r = RedTeam.leader.row;

            if (board.getCellStatus(c, r) == (int)Board.Status.PIECE_AREA)
            {
                RedTeam.leader.hasPiece = true;
                foreach (Piece p in board.Pieces)
                {
                    if (p.row == r && p.column == c)
                    {
                        board.Pieces.Remove(p);
                        return;
                    }
                }
            }else if(board.getCellStatus(c,r) == (int)Board.Status.UNDISCOVERED_RED_GOALS && RedTeam.leader.hasPiece)
            {
                foreach (Goal redGoalDiscoverdByPlayer in board.undiscoveredRedGoals)
                {
                    if (redGoalDiscoverdByPlayer.row == r && redGoalDiscoverdByPlayer.column == c)
                    {
                        board.undiscoveredRedGoals.Remove(redGoalDiscoverdByPlayer);
                        board.discoveredRedGoals.Add(redGoalDiscoverdByPlayer);
                        RedTeam.leader.hasPiece = false;
                        return;
                    }
                }
            }

            c = BlueTeam.leader.column;
            r = BlueTeam.leader.row;
            if (board.getCellStatus(c, r) == (int)Board.Status.PIECE_AREA)
            {
                BlueTeam.leader.hasPiece = true;
                foreach (Piece p in board.Pieces)
                {
                    if (p.row == r && p.column == c)
                    {
                        board.Pieces.Remove(p);
                        return;
                    }
                }
            }
            else if (board.getCellStatus(c, r) == (int)Board.Status.UNDISCOVERED_BLUE_GOALS && BlueTeam.leader.hasPiece)
            {
                foreach (Goal blueGoalDiscoverdByPlayer in board.undiscoveredBlueGoals)
                {
                    if (blueGoalDiscoverdByPlayer.row == r && blueGoalDiscoverdByPlayer.column == c)
                    {
                        board.undiscoveredBlueGoals.Remove(blueGoalDiscoverdByPlayer);
                        board.discoveredBlueGoals.Add(blueGoalDiscoverdByPlayer);
                        BlueTeam.leader.hasPiece = false;
                        return;
                    }
                }
            }
        }
        #endregion

        /**Logic for Goals**/
        private void initGoals() {


            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 0; i < board.NumberOfGoals; i++)
            {
                Goal goal = new Goal();
                goal.row = rnd.Next(0, Board.GoalHeight);
                goal.column = rnd.Next(0, Board.Width);
                board.undiscoveredRedGoals.Add(goal);
            }

            for (int i = 0; i < board.NumberOfGoals; i++)
            {
                Goal goal = new Goal();
                goal.row = rnd.Next(Board.Height - Board.GoalHeight, Board.Height);
                goal.column = rnd.Next(0, Board.Width);
                board.undiscoveredBlueGoals.Add(goal);
            }
        }

        private void loadBoard()
        {
            Board.Width = 6;   
            Board.GoalHeight = 3;
            Board.TaskHeight = 4;
            Board.Height = 2* Board.GoalHeight + Board.TaskHeight;
            board.InitialNumberOfPieces = 10;
            board.NumberOfGoals = 2;

            board.undiscoveredRedGoals = new List<Goal>();
            board.undiscoveredBlueGoals = new List<Goal>();
            board.discoveredRedGoals = new List<Goal>();
            board.discoveredBlueGoals = new List<Goal>();
            board.nonGoals = new List<Goal>();

            RedTeam = loadRedTeam();
            board.RedTeam = RedTeam;

            BlueTeam = loadBlueTeam();
            board.BlueTeam = BlueTeam;

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

            for (int i = 0; i < board.InitialNumberOfPieces; i++)
            {
                Piece piece = new Piece();
                piece.row = rnd.Next(Board.GoalHeight, Board.GoalHeight+Board.TaskHeight);
                piece.column = rnd.Next(0, Board.Width);
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
                Team = Team.TeamColor.BLUE
            };

            team.members = new List<Player>();
            //team.members.Add(new Player {
            //    role = Player.Role.MEMBER,
            //    playerID = 12,
            //    row = 7,
            //    column = 3,
            //    Team = Team.TeamColor.BLUE
            //});

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
                Team = Team.TeamColor.RED
            };

            team.members = new List<Player>();
            //team.members.Add(new Player
            //{
            //    role = Player.Role.MEMBER,
            //    playerID = 12,
            //    row = 1,
            //    column = 3,
            //    Team = Team.TeamColor.RED
            //});

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


                        case (int)Board.Status.UNDISCOVERED_BLUE_GOALS:
                        case (int)Board.Status.UNDISCOVERED_RED_GOALS:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/undiscovered_goal.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.DISCOVERED_BLUE_GOALS:
                        case (int)Board.Status.DISCOVERED_RED_GOALS:
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
    }
}
