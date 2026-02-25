using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using ChessGameMAUI.ViewModels.Base;
using Microsoft.Maui.Controls;
using System.IO;
using Microsoft.Maui.Storage;
using System.Windows.Input;



namespace ChessGameMAUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IServicePartie _servicePartie;

        private string _messageStatut;
        private bool _afficherCoordonnees;
        private bool _afficherCoupsPossibles;

        public EchiquierViewModel EchiquierViewModel { get; }
        public InfoPartieViewModel InfoPartieViewModel { get; }
        


        public string MessageStatut
        {
            get => _messageStatut;
            set => SetProperty(ref _messageStatut, value);
        }

        public bool AfficherCoordonnees
        {
            get => _afficherCoordonnees;
            set => SetProperty(ref _afficherCoordonnees, value);
        }

        public bool AfficherCoupsPossibles
        {
            get => _afficherCoupsPossibles;
            set => SetProperty(ref _afficherCoupsPossibles, value);
        }

        // Commandes
        public ICommand CommandeNouvellePartie { get; }
        public ICommand CommandeChargerPartie { get; }
        public ICommand CommandeSauvegarderPartie { get; }
        public ICommand CommandeQuitter { get; }
        public ICommand CommandeAnnulerCoup { get; }
        public ICommand CommandeRefaireCoup { get; }
        public ICommand CommandeProposerNulle { get; }
        public ICommand CommandeAbandonner { get; }
  
        public MainViewModel(IServicePartie servicePartie)
        {
            // Service central (logique métier)
            _servicePartie = servicePartie;

            // Sous-viewmodels
            EchiquierViewModel = new EchiquierViewModel(_servicePartie);
            InfoPartieViewModel = new InfoPartieViewModel(_servicePartie);

            // États initiaux
            AfficherCoordonnees = true;
            AfficherCoupsPossibles = true;
            MessageStatut = "Bienvenue !";

            // Commandes
            CommandeNouvellePartie = new Command(async () => await NouvellePartie());
            CommandeChargerPartie = new Command(async () => await ChargerPartie());
            CommandeSauvegarderPartie = new Command(async () => await SauvegarderPartie());
            CommandeQuitter = new Command(Quitter);
            CommandeAnnulerCoup = new Command(AnnulerCoup);
            CommandeRefaireCoup = new Command(RefaireCoup);
            CommandeProposerNulle = new Command(() => InfoPartieViewModel.CommandeProposerNulle.Execute(null));
            CommandeAbandonner = new Command(() => InfoPartieViewModel.CommandeAbandonner.Execute(null));

            


            // Première partie
            DemarrerPartieDefaut();
        }



        // -------------------------
        //  MÉTHODES PRINCIPALES
        // -------------------------

        private void DemarrerPartieDefaut()
        {
            _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);
            EchiquierViewModel.ActualiserEchiquier();
            InfoPartieViewModel.ActualiserInfos();
        }

        private async Task NouvellePartie()
        {
            bool reponse = await Application.Current.MainPage.DisplayAlert(
                "Nouvelle partie",
                "Voulez-vous démarrer une nouvelle partie ?",
                "Oui", "Non");

            if (!reponse) return;

            _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);

            EchiquierViewModel.ReinitialiserEchiquier();
            InfoPartieViewModel.ActualiserInfos();

            MessageStatut = "Nouvelle partie démarrée.";
        }

        private async Task ChargerPartie()
        {
            var fichier = await FilePicker.PickAsync();
            if (fichier == null)
                return;

            bool succes = _servicePartie.ChargerPartie(fichier.FullPath);

            if (succes)
                await Application.Current.MainPage.DisplayAlert("OK", "Partie chargée.", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("Erreur", "Impossible de charger.", "OK");

            EchiquierViewModel.ActualiserEchiquier();
            InfoPartieViewModel.ActualiserInfos();
        }

        private async Task SauvegarderPartie()
        {
            string fileName = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}.pgn";

            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                fileName);

            bool succes = _servicePartie.SauvegarderPartie(filePath);

            if (succes)
                await Application.Current.MainPage.DisplayAlert("Sauvegarde", $"Fichier : {filePath}", "OK");
            else
                await Application.Current.MainPage.DisplayAlert("Erreur", "Erreur lors de la sauvegarde.", "OK");
        }

        private void Quitter()
        {
#if WINDOWS
            Application.Current.Quit();
#endif
        }

        private void AnnulerCoup()
        {
            if (_servicePartie.AnnulerCoup())
            {
                EchiquierViewModel.ActualiserEchiquier();
                InfoPartieViewModel.ActualiserInfos();

                MessageStatut = "Coup annulé.";
            }
        }

        private void RefaireCoup()
        {
            if (_servicePartie.RefaireCoup())
            {
                EchiquierViewModel.ActualiserEchiquier();
                InfoPartieViewModel.ActualiserInfos();

                MessageStatut = "Coup refait.";
            }
            else
            {
                MessageStatut = "Aucun coup à refaire.";
            }
        }
    }
}
