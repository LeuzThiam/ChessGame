using ChessGame.Services;
using ChessGame.Services.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace ChessGame.Views
{
    public partial class ChoixJoueursView : UserControl
    {
        public ChoixJoueursView()
        {
            InitializeComponent();
        }

        private void BtnDemarrer_Click(object sender, RoutedEventArgs e)
        {
            string blanc = TxtBlanc.Text.Trim();
            string noir = TxtNoir.Text.Trim();

            if (string.IsNullOrWhiteSpace(blanc) || string.IsNullOrWhiteSpace(noir))
            {
                MessageBox.Show("Veuillez remplir les deux noms.");
                return;
            }

            // Créer le service de partie
            IServicePartie service = new ServicePartie();
            service.DemarrerNouvellePartie(blanc, noir, 10);

            // Créer l'échiquier
            EchiquierView plateau = new EchiquierView();
            plateau.Initialiser(service);

            // Changer de vue
            MainWindow.Instance.ChangerVue(plateau);
            MainWindow.Instance.DemarrerPartie(blanc, noir);

        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.ChangerVue(new MainMenuView());
        }


    }
}
