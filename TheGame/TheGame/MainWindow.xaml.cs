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

        public MainWindow()
        {
            InitializeComponent();
            board = new Board();
            loadBoard();
            updateBoard();

        }

        private void loadBoard()
        {
            board.Width = 5;
            board.Height = 9;
            board.GoalWidth = 2;
            board.RedTeam = loadRedTeam();
            board.BlueTeam = loadBlueTeam();

        }

        private Team loadBlueTeam()
        {
            Team team = new Team();
            team.leader = new Player
            {
                role = Player.Role.leader,
                playerID = 10,
                row = 7,
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
                    Height = new GridLength(50)
                };
                playgroundDockPanel.RowDefinitions.Add(rowdef);
            }
            for (int col = 0; col < board.Width ; col++)
            {
                ColumnDefinition coldef = new ColumnDefinition
                {
                    Width = new GridLength(50)
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
                        case (int)Board.Status.BLUE_GOAL:
                        case (int)Board.Status.RED_GOAL:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.RED_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/red_player.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.BLUE_PLAYER:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/blue_player.png", UriKind.Relative));
                            break;

                        case (int)Board.Status.SIMPE_AREA:
                        default:
                            img.Source = new BitmapImage(new Uri("/TheGame;component/Image/white_picture.png", UriKind.Relative));
                            break;
                    }

                    
                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);

                    playgroundDockPanel.Children.Add(img);
                }
            }
        }
    }
}
