using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private GameViewModel ViewModel { get => this.DataContext as GameViewModel; }

        private int GetSquareIndex(object sender)
        {
            var contentPresenter = VisualTreeHelper.GetParent(sender as DependencyObject);
            return Board.ItemContainerGenerator.IndexFromContainer(contentPresenter);
        }

        private void Square_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.HandleSquareMouseUp(this.GetSquareIndex(sender));
        }

        private void Square_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ViewModel.HandleSquareMouseEnter(this.GetSquareIndex(sender));
        }

        private void Square_MouseLeave(object sender, MouseEventArgs e)
        {
            // For some reason, squareIndex is -1 immediately after a move.
            var squareIndex = this.GetSquareIndex(sender);
            if (squareIndex != -1)
            {
                this.ViewModel.HandleSquareMouseLeave(this.GetSquareIndex(sender));
            }
        }
    }
}
