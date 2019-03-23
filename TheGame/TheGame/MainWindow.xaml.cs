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

            int c = RedTeam.leader.column;
            int r = RedTeam.leader.row;
            takePiece(RedTeam.leader);

            //If the red player steps in the undiscovered red goal cell and he has a piece
            if (board.getCellStatus(c, r) == (int)Board.Status.UNDISCOVERED_RED_GOALS && RedTeam.leader.hasPiece)
            {
                foreach (Goal redGoalDiscoverdByPlayer in board.UndiscoveredRedGoals)
                {
                    if (redGoalDiscoverdByPlayer.row == r && redGoalDiscoverdByPlayer.column == c)
                    {
                        board.UndiscoveredRedGoals.Remove(redGoalDiscoverdByPlayer); //Since goal has been discoverd, remove it.
                        board.DiscoveredRedGoals.Add(redGoalDiscoverdByPlayer); //Add "YG" to that (goal has been discoverd).
                        RedTeam.leader.hasPiece = false; //Player no longer has the piece.
                        return;
                    }
                }
            }
            PlayerDiscoversNeighboringCells(RedTeam.leader);

            c = BlueTeam.leader.column;
            r = BlueTeam.leader.row;
            takePiece(BlueTeam.leader);
            if (board.getCellStatus(c, r) == (int)Board.Status.UNDISCOVERED_BLUE_GOALS && BlueTeam.leader.hasPiece)
            {
                foreach (Goal blueGoalDiscoverdByPlayer in board.UndiscoveredBlueGoals)
                {
                    if (blueGoalDiscoverdByPlayer.row == r && blueGoalDiscoverdByPlayer.column == c)
                    {
                        board.UndiscoveredBlueGoals.Remove(blueGoalDiscoverdByPlayer);
                        board.DiscoveredBlueGoals.Add(blueGoalDiscoverdByPlayer);
                        BlueTeam.leader.hasPiece = false;
                        return;
                    }
                }
            }
            PlayerDiscoversNeighboringCells(BlueTeam.leader);

        }

        /* Player discovers its 8 neighbors */
        private void PlayerDiscoversNeighboringCells(Player player)
        {
            for (int c = 0; c < 3; c++)
                for (int r = 0; r < 3; r++)
                    player.Neighbors[c, r] = board.GetPlayersNeighbor(player.column-1+c, player.row-1+r, player.Team);

        }
        #endregion
               
        /** Player takes a  Piece **/
        private void takePiece(Player player)
        {
            int c = player.column;
            int r = player.row;

            foreach (Piece p in board.Pieces)
            {
                if (p.row == r && p.column == c)
                {
                    player.hasPiece = true;
                    board.Pieces.Remove(p);
                    return;
                }
            }
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
            Board.Width = 6;   
            Board.GoalHeight = 3;
            Board.TaskHeight = 4;
            Board.Height = 2* Board.GoalHeight + Board.TaskHeight;
            board.InitialNumberOfPieces = 10;
            board.NumberOfGoals = 2;

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
                Team = Team.TeamColor.BLUE,
                Neighbors = new Player.NeighborStatus[3, 3]
            };

            team.members = new List<Player>();
            //team.members.Add(new Player {
            //    role = Player.Role.MEMBER,
            //    playerID = 12,
            //    row = 7,
            //    column = 3,
            //    Team = Team.TeamColor.BLUE,
            //    Neighbors = new Player.NeighborStatus[3,3]
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
                Team = Team.TeamColor.RED,
                Neighbors = new Player.NeighborStatus[3,3]
            };

            team.members = new List<Player>();
            //team.members.Add(new Player
            //{
            //    role = Player.Role.MEMBER,
            //    playerID = 12,
            //    row = 1,
            //    column = 3,
            //    Team = Team.TeamColor.RED,
            //    Neighbors = new Player.NeighborStatus[3,3]
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
