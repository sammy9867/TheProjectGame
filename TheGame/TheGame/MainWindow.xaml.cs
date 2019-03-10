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
        public const int WIDTH_BOARD = 5;
        public const int HEIGHT_BOARD = 5; 

        public MainWindow()
        {
            InitializeComponent();

            loadBoard();

        }

        private void loadBoard()
        {
            
            for(int i=0; i<WIDTH_BOARD; i++)
                for(int j=0; j<HEIGHT_BOARD; j++)
                {
                    Image img = new Image
                    {
                        Width = 100,
                        Height = 100,
                        Visibility = Visibility.Visible
                    };
                    img.Source = ((i + j) % 2 == 0) ? new BitmapImage(new Uri("/TheGame;component/Image/white_picture.png", UriKind.Relative)) :
                                                      new BitmapImage(new Uri("/TheGame;component/Image/brown_picture.png", UriKind.Relative));
                    ColumnDefinition column = new ColumnDefinition();
                    column.Width = new GridLength(this.Width*0.64 / WIDTH_BOARD);
                    playgroundDockPanel.ColumnDefinitions.Add(column);

                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(this.Height*0.8 / HEIGHT_BOARD);
                    playgroundDockPanel.RowDefinitions.Add(row);


                    Grid.SetColumn(img, j);
                    Grid.SetRow(img, i);

                    playgroundDockPanel.Children.Add(img);
                }
        }
    }
}
