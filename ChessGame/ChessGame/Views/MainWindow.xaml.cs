using System.Windows;
using ChessGame.ViewModels;

namespace ChessGame.Views
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Définir le DataContext avec le ViewModel principal
            DataContext = new MainViewModel();
        }
    }
}