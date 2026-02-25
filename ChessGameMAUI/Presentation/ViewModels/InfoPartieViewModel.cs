using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using ChessGameMAUI.ViewModels.Base;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
#if WINDOWS
using Microsoft.Maui.Platform;
#endif
using Microsoft.Maui.Storage;

namespace ChessGameMAUI.ViewModels
{
    public class InfoPartieViewModel : ViewModelBase
    {
        private readonly IServicePartie _servicePartie;

        private string _nomJoueurBlanc = string.Empty;
        private string _nomJoueurNoir = string.Empty;
        private string _tempsJoueurBlanc = string.Empty;
        private string _tempsJoueurNoir = string.Empty;
        private string _statutPartie = string.Empty;
        private string _tourActuel = string.Empty;
        private string _capturesParNoirs = "⚪ capturées par noirs : -";
        private string _capturesParBlancs = "⚫ capturées par blancs : -";

        public string NomJoueurBlanc
        {
            get => _nomJoueurBlanc;
            set => SetProperty(ref _nomJoueurBlanc, value);
        }

        public string NomJoueurNoir
        {
            get => _nomJoueurNoir;
            set => SetProperty(ref _nomJoueurNoir, value);
        }

        public string TempsJoueurBlanc
        {
            get => _tempsJoueurBlanc;
            set => SetProperty(ref _tempsJoueurBlanc, value);
        }

        public string TempsJoueurNoir
        {
            get => _tempsJoueurNoir;
            set => SetProperty(ref _tempsJoueurNoir, value);
        }

        public string StatutPartie
        {
            get => _statutPartie;
            set => SetProperty(ref _statutPartie, value);
        }

        public string TourActuel
        {
            get => _tourActuel;
            set => SetProperty(ref _tourActuel, value);
        }

        public string CapturesParNoirs
        {
            get => _capturesParNoirs;
            set => SetProperty(ref _capturesParNoirs, value);
        }

        public string CapturesParBlancs
        {
            get => _capturesParBlancs;
            set => SetProperty(ref _capturesParBlancs, value);
        }

        public ObservableCollection<string> HistoriqueCoups { get; } = new();

        public ICommand CommandeNouvellePartie { get; }
        public ICommand CommandeAnnulerCoup { get; }
        public ICommand CommandeProposerNulle { get; }
        public ICommand CommandeAbandonner { get; }
        public ICommand CommandeSauvegarder { get; }
        public ICommand CommandeCharger { get; }

        public InfoPartieViewModel(IServicePartie servicePartie)
        {
            _servicePartie = servicePartie;

            CommandeNouvellePartie = new RelayCommand(async _ => await NouvellePartie());
            CommandeAnnulerCoup = new RelayCommand(_ => AnnulerCoup(), _ => PeutAnnulerCoup());
            CommandeProposerNulle = new RelayCommand(async _ => await ProposerNulle(), _ => PartieEnCours());
            CommandeAbandonner = new RelayCommand(async _ => await Abandonner(), _ => PartieEnCours());
            CommandeSauvegarder = new RelayCommand(async _ => await Sauvegarder(), _ => PartieEnCours());
            CommandeCharger = new RelayCommand(async _ => await Charger());

            _servicePartie.CoupJoue += (s, e) => ActualiserInfos();
            _servicePartie.CoupAnnule += (s, e) => ActualiserInfos();
            _servicePartie.PartieTerminee += (s, e) => ActualiserInfos();
            _servicePartie.StatutPartieChange += (s, e) => ActualiserInfos();

            ActualiserInfos();
        }

        public void ActualiserInfos()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var etat = _servicePartie.EtatPartie;
                if (etat == null)
                    return;

                NomJoueurBlanc = etat.JoueurBlanc?.Nom ?? "Joueur Blanc";
                NomJoueurNoir = etat.JoueurNoir?.Nom ?? "Joueur Noir";

                TempsJoueurBlanc = FormaterTemps(etat.JoueurBlanc?.TempsRestant ?? TimeSpan.Zero);
                TempsJoueurNoir = FormaterTemps(etat.JoueurNoir?.TempsRestant ?? TimeSpan.Zero);

                StatutPartie = ObtenirTexteStatut(_servicePartie.ObtenirStatutPartie());

                var joueurActif = _servicePartie.ObtenirJoueurActif();
                TourActuel = joueurActif != null
                    ? $"Trait aux {joueurActif.Couleur}"
                    : "Partie terminée";

                ActualiserHistorique();
                ActualiserCaptures();
            });
        }

        private void ActualiserHistorique()
        {
            HistoriqueCoups.Clear();

            var coups = _servicePartie.ObtenirHistorique();
            if (coups == null || coups.Count == 0)
                return;

            for (int i = 0; i < coups.Count; i++)
            {
                if (i % 2 == 0)
                {
                    string ligne = $"{(i / 2) + 1}. {coups[i].NotationAlgebrique}";

                    if (i + 1 < coups.Count)
                    {
                        ligne += $"   {coups[i + 1].NotationAlgebrique}";
                        i++;
                    }

                    HistoriqueCoups.Add(ligne);
                }
            }
        }

        private void ActualiserCaptures()
        {
            var coups = _servicePartie.ObtenirHistorique();
            if (coups == null || coups.Count == 0)
            {
                CapturesParNoirs = "⚪ capturées par noirs : -";
                CapturesParBlancs = "⚫ capturées par blancs : -";
                return;
            }

            var blancsCaptures = new List<string>();
            var noirsCaptures = new List<string>();

            foreach (var coup in coups)
            {
                if (coup.PieceCapturee == null)
                    continue;

                var symbole = ObtenirSymboleUnicode(coup.PieceCapturee.Type, coup.PieceCapturee.Couleur);
                if (coup.PieceCapturee.Couleur == CouleurPiece.Blanc)
                    blancsCaptures.Add(symbole);
                else
                    noirsCaptures.Add(symbole);
            }

            CapturesParNoirs = blancsCaptures.Count > 0
                ? $"⚪ capturées par noirs : {string.Join("", blancsCaptures)}"
                : "⚪ capturées par noirs : -";

            CapturesParBlancs = noirsCaptures.Count > 0
                ? $"⚫ capturées par blancs : {string.Join("", noirsCaptures)}"
                : "⚫ capturées par blancs : -";
        }

        private async Task NouvellePartie()
        {
            bool ok = await Shell.Current.DisplayAlert(
                "Nouvelle partie",
                "Voulez-vous vraiment démarrer une nouvelle partie ?",
                "Oui", "Non");

            if (!ok) return;

            _servicePartie.DemarrerNouvellePartie("Blancs", "Noirs", 10);
            ActualiserInfos();
        }

        private void AnnulerCoup()
        {
            if (_servicePartie.AnnulerCoup())
                ActualiserInfos();
        }

        private async Task ProposerNulle()
        {
            bool ok = await Shell.Current.DisplayAlert(
                "Proposer nulle",
                "Voulez-vous proposer une partie nulle ?",
                "Oui", "Non");

            if (!ok) return;

            _servicePartie.ProposerNulle();

            bool accepte = await Shell.Current.DisplayAlert(
                "Réponse",
                "L'adversaire accepte-t-il la nulle ?",
                "Oui", "Non");

            if (accepte) _servicePartie.AccepterNulle();
            else _servicePartie.RefuserNulle();

            ActualiserInfos();
        }

        private async Task Abandonner()
        {
            var joueur = _servicePartie.ObtenirJoueurActif();
            if (joueur == null) return;

            bool ok = await Shell.Current.DisplayAlert(
                "Abandon",
                $"{joueur.Nom} abandonne ?",
                "Oui", "Non");

            if (!ok) return;

            _servicePartie.Abandonner(joueur.Couleur);
            ActualiserInfos();
        }

        private async Task Sauvegarder()
        {
            string chemin;

#if WINDOWS
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedFileName = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}";
            savePicker.FileTypeChoices.Add("Fichier PGN", new List<string>() { ".pgn" });
            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var fichier = await savePicker.PickSaveFileAsync();
            if (fichier == null)
                return;

            chemin = fichier.Path;
#else
            chemin = System.IO.Path.Combine(
                FileSystem.AppDataDirectory,
                $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}.pgn");
#endif

            bool success = _servicePartie.SauvegarderPartie(chemin);

            if (success)
                await Shell.Current.DisplayAlert("Succès", "Partie sauvegardée.", "OK");
            else
                await Shell.Current.DisplayAlert("Erreur", "Échec de la sauvegarde.", "OK");
        }

        private async Task Charger()
        {
            var fichier = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Charger une partie",
                FileTypes = null
            });

            if (fichier == null)
                return;

            bool success = _servicePartie.ChargerPartie(fichier.FullPath);

            if (success)
            {
                ActualiserInfos();
                await Shell.Current.DisplayAlert("Succès", "Partie chargée.", "OK");
            }
            else
                await Shell.Current.DisplayAlert("Erreur", "Échec du chargement.", "OK");
        }

        private string FormaterTemps(TimeSpan temps)
        {
            return temps.TotalHours >= 1
                ? temps.ToString(@"hh\:mm\:ss")
                : temps.ToString(@"mm\:ss");
        }

        private string ObtenirTexteStatut(ChessGame.Core.Domain.Models.StatutPartie statut)
        {
            return statut switch
            {
                ChessGame.Core.Domain.Models.StatutPartie.EnCours => "Partie en cours",
                ChessGame.Core.Domain.Models.StatutPartie.EchecBlanc => "Échec au roi blanc",
                ChessGame.Core.Domain.Models.StatutPartie.EchecNoir => "Échec au roi noir",
                ChessGame.Core.Domain.Models.StatutPartie.EchecEtMatBlanc => "Échec et mat ! Les noirs gagnent",
                ChessGame.Core.Domain.Models.StatutPartie.EchecEtMatNoir => "Échec et mat ! Les blancs gagnent",
                ChessGame.Core.Domain.Models.StatutPartie.Pat => "Pat - partie nulle",
                ChessGame.Core.Domain.Models.StatutPartie.Nulle => "Partie nulle",
                ChessGame.Core.Domain.Models.StatutPartie.AbandonBlanc => "Les blancs abandonnent",
                ChessGame.Core.Domain.Models.StatutPartie.AbandonNoir => "Les noirs abandonnent",
                _ => "Statut inconnu"
            };
        }

        private bool PeutAnnulerCoup()
        {
            var liste = _servicePartie.ObtenirHistorique();
            return liste != null && liste.Count > 0 && !_servicePartie.EstPartieTerminee();
        }

        private bool PartieEnCours()
        {
            return !_servicePartie.EstPartieTerminee();
        }

        private string ObtenirSymboleUnicode(TypePiece type, CouleurPiece couleur)
        {
            return couleur == CouleurPiece.Blanc
                ? type switch
                {
                    TypePiece.Pion => "♙",
                    TypePiece.Cavalier => "♘",
                    TypePiece.Fou => "♗",
                    TypePiece.Tour => "♖",
                    TypePiece.Reine => "♕",
                    TypePiece.Roi => "♔",
                    _ => "♙"
                }
                : type switch
                {
                    TypePiece.Pion => "♟",
                    TypePiece.Cavalier => "♞",
                    TypePiece.Fou => "♝",
                    TypePiece.Tour => "♜",
                    TypePiece.Reine => "♛",
                    TypePiece.Roi => "♚",
                    _ => "♟"
                };
        }
    }
}
