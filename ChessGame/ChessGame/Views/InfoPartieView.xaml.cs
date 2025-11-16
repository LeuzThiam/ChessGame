using ChessGame.Services.Interfaces;
using ChessGame.ViewModels;
using System.Windows.Controls;

namespace ChessGame.Views
{
    public partial class InfoPartieView : UserControl
    {
        private InfoPartieViewModel _viewModel;

        public InfoPartieView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Méthode appelée depuis MainWindow pour injecter le service.
        /// </summary>
        public void Initialiser(IServicePartie service)
        {
            // Création du ViewModel (un seul)
            _viewModel = new InfoPartieViewModel(service);

            // Assignation au DataContext
            DataContext = _viewModel;

            // Mise à jour de l'affichage au démarrage
            _viewModel.ActualiserInfos();
        }
    }
}
