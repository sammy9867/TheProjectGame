using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /**Static Game Settings**/
        double probabilityOfBeingSham { get; set; }
        int frequencyOfPlacingPieces { get; set; }
        int initialNumberOfPieces { get; set; }
        int tasksHeight { get; set; } //length of task area
        int goalHeight { get; set; } //length of single goal area
        int numberOfPlayersPerTeam { get; set; }
        int goalDefinition { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            board = new Board();
            loadBoard();
         //   loadInitSettings();
            loadPieces();
            updateBoard();

        }




/**
        //TO BE CHANGED WITH USER INPUT/JSON LATER ON.
        private void loadInitSettings()
        {
            probabilityOfBeingSham = 0.2;
            frequencyOfPlacingPieces = 1500; //what's this?
            initialNumberOfPieces = 5;
            tasksHeight = 5;
           // goalHeight = 2;
            numberOfPlayersPerTeam = 1;
            goalDefinition = 3;

        } **/




        //FIX DIMENSIONS PROB.
        private void loadBoard()
        {
            board.Width = 6;   
            board.GoalHeight = 2;
            board.TaskHeight = 4;
            board.Height = 2* board.GoalHeight + board.TaskHeight;
            board.RedTeam = loadRedTeam();
            board.BlueTeam = loadBlueTeam();
            board.pieces = loadPieces();
        }

        private List<Piece> loadPieces()
        {
            List<Piece> pieces = new List<Piece>();

            initialNumberOfPieces = 5;
            Random rnd = new Random();
            

            for (int i = 0; i < initialNumberOfPieces; i++)
            {
                Piece piece = new Piece();
                piece.row = rnd.Next(2, 6);
                piece.column = rnd.Next(0, 6);
                pieces.Add(piece);
               
            }
            return pieces;

        }

        private Team loadBlueTeam()
        {
            Team team = new Team();
            team.leader = new Player
            {
                role = Player.Role.leader,
                playerID = 10,
                row = 6,
                column = 1
            };

            team.members = new List<Player>();
            team.members.Add(new Player {
                role = Player.Role.member,
                playerID = 12,
                row = 7,
                column = 3
            });

            team.teamColor = Team.TeamColor.blue;

            team.maxNumOfPlayers = 2;

            return team;
        }

        private Team loadRedTeam()
        {
            Team team = new Team();
            team.leader = new Player
            {
                role = Player.Role.leader,
                playerID = 10,
                row = 1,
                column = 1
            };

            team.members = new List<Player>();
            team.members.Add(new Player
            {
                role = Player.Role.member,
                playerID = 12,
                row = 1,
                column = 3
            });

            team.teamColor = Team.TeamColor.red;

            team.maxNumOfPlayers = 2;

            return team;
        }

        private void updateBoard()
        {
            for (int row = 0; row < board.Height ; row++)
            {
                RowDefinition rowdef = new RowDefinition
                {
                    Height = new GridLength(100)
                };
                playgroundDockPanel.RowDefinitions.Add(rowdef);
            }
            for (int col = 0; col < board.Width ; col++)
            {
                ColumnDefinition coldef = new ColumnDefinition
                {
                    Width = new GridLength(100),
   
                };

                playgroundDockPanel.ColumnDefinitions.Add(coldef);
            }

            for (int row = 0; row < board.Height; row++)
            {
                for (int col = 0; col < board.Width; col++)
                {
                    Image img = new Image
                    {
                        Width = 100,
                        Height = 100,
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
                    img.Stretch = Stretch.Uniform;


                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);

                    playgroundDockPanel.Children.Add(img);
                }
            }
        }
    }
}
