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
            board = new Board { Width = 5, Height = 5 };
            loadBoard();

        }

        private void loadBoard()
        {
            
            for(int i=0; i<board.Width; i++)
                for(int j=0; j<board.Height; j++)
                {
                    Image img = new Image
                    {
                        Width = 100,
                        Height = 100,
                        Visibility = Visibility.Visible,
                        Source = ((i + j) % 2 == 0) ? new BitmapImage(new Uri("/TheGame;component/Image/white_picture.png", UriKind.Relative)) :
                                                      new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative)),
                        Tag = ""+i+"x"+j
                };
                    
                    ColumnDefinition column = new ColumnDefinition
                    {
                        Width = new GridLength(this.Width * 0.64 / board.Width)
                    };
                    playgroundDockPanel.ColumnDefinitions.Add(column);

                    RowDefinition row = new RowDefinition
                    {
                        Height = column.Width
                    };
                    playgroundDockPanel.RowDefinitions.Add(row);


                    Grid.SetColumn(img, j);
                    Grid.SetRow(img, i);

                    playgroundDockPanel.Children.Add(img);
                }
        }
    }
}
