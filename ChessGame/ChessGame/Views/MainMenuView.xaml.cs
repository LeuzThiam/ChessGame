using System.Windows;
using System.Windows.Controls;

namespace ChessGame.Views
{
    public partial class MainMenuView : UserControl
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        private void BtnJouer_Click(object sender, RoutedEventArgs e)
        {
            // Aller à la page choix joueur
            MainWindow.Instance.ChangerVue(new ChoixJoueursView());
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
