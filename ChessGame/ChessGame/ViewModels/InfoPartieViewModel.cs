using ChessGame.Models;
using ChessGame.Services.Interfaces;
using ChessGame.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// ViewModel pour le panneau d'informations de la partie
    /// </summary>
    public class InfoPartieViewModel : ViewModelBase
    {
        private readonly IServicePartie _servicePartie;
        private string _nomJoueurBlanc;
        private string _nomJoueurNoir;
        private string _tempsJoueurBlanc;
        private string _tempsJoueurNoir;
        private string _statutPartie;
        private string _tourActuel;

        #region Propriétés

        /// <summary>
        /// Nom du joueur blanc
        /// </summary>
        public string NomJoueurBlanc
        {
            get => _nomJoueurBlanc;
            set => SetProperty(ref _nomJoueurBlanc, value);
        }

        /// <summary>
        /// Nom du joueur noir
        /// </summary>
        public string NomJoueurNoir
        {
            get => _nomJoueurNoir;
            set => SetProperty(ref _nomJoueurNoir, value);
        }

        /// <summary>
        /// Temps restant du joueur blanc
        /// </summary>
        public string TempsJoueurBlanc
        {
            get => _tempsJoueurBlanc;
            set => SetProperty(ref _tempsJoueurBlanc, value);
        }

        /// <summary>
        /// Temps restant du joueur noir
        /// </summary>
        public string TempsJoueurNoir
        {
            get => _tempsJoueurNoir;
            set => SetProperty(ref _tempsJoueurNoir, value);
        }

        /// <summary>
        /// Statut actuel de la partie
        /// </summary>
        public string StatutPartie
        {
            get => _statutPartie;
            set => SetProperty(ref _statutPartie, value);
        }

        /// <summary>
        /// Tour actuel
        /// </summary>
        public string TourActuel
        {
            get => _tourActuel;
            set => SetProperty(ref _tourActuel, value);
        }

        /// <summary>
        /// Historique des coups
        /// </summary>
        public ObservableCollection<string> HistoriqueCoups { get; }

        #endregion

        #region Commandes

        public ICommand CommandeNouvellePartie { get; }
        public ICommand CommandeAnnulerCoup { get; }
        public ICommand CommandeProposerNulle { get; }
        public ICommand CommandeAbandonner { get; }
        public ICommand CommandeSauvegarder { get; }
        public ICommand CommandeCharger { get; }

        #endregion

        #region Constructeur

        public InfoPartieViewModel(IServicePartie servicePartie)
        {
            _servicePartie = servicePartie;

            HistoriqueCoups = new ObservableCollection<string>();

            // Initialiser les commandes
            CommandeNouvellePartie = new RelayCommand(_ => NouvellePartie());
            CommandeAnnulerCoup = new RelayCommand(_ => AnnulerCoup(), _ => PeutAnnulerCoup());
            CommandeProposerNulle = new RelayCommand(_ => ProposerNulle(), _ => PartieEnCours());
            CommandeAbandonner = new RelayCommand(_ => Abandonner(), _ => PartieEnCours());
            CommandeSauvegarder = new RelayCommand(_ => Sauvegarder());
            CommandeCharger = new RelayCommand(_ => Charger());

            // S'abonner aux événements du service
            _servicePartie.CoupJoue += (s, e) => ActualiserInfos();
            _servicePartie.CoupAnnule += (s, e) => ActualiserInfos();
            _servicePartie.PartieTerminee += (s, e) => ActualiserInfos();
            _servicePartie.StatutPartieChange += (s, e) => ActualiserInfos();
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Actualise toutes les informations affichées
        /// </summary>
        public void ActualiserInfos()
        {
            if (_servicePartie.EtatPartie == null)
                return;

            // Noms des joueurs
            NomJoueurBlanc = _servicePartie.EtatPartie.JoueurBlanc?.Nom ?? "Joueur Blanc";
            NomJoueurNoir = _servicePartie.EtatPartie.JoueurNoir?.Nom ?? "Joueur Noir";

            // Temps
            TempsJoueurBlanc = FormateurTemps(_servicePartie.EtatPartie.JoueurBlanc?.TempsRestant ?? TimeSpan.Zero);
            TempsJoueurNoir = FormateurTemps(_servicePartie.EtatPartie.JoueurNoir?.TempsRestant ?? TimeSpan.Zero);

            // Statut
            StatutPartie = ObtenirTexteStatut(_servicePartie.ObtenirStatutPartie());

            // Tour actuel
            var joueurActif = _servicePartie.ObtenirJoueurActif();
            TourActuel = joueurActif != null
                ? $"Tour: {joueurActif.Nom} ({joueurActif.Couleur})"
                : "Partie terminée";

            // Historique
            ActualiserHistorique();
        }

        /// <summary>
        /// Actualise l'historique des coups
        /// </summary>
        private void ActualiserHistorique()
        {
            HistoriqueCoups.Clear();

            var coups = _servicePartie.ObtenirHistorique();
            if (coups == null || coups.Count == 0)
                return;

            for (int i = 0; i < coups.Count; i++)
            {
                // Ajouter le numéro de coup pour les blancs
                if (i % 2 == 0)
                {
                    string ligne = $"{(i / 2) + 1}. {coups[i].NotationAlgebrique}";

                    // Ajouter le coup des noirs sur la même ligne si disponible
                    if (i + 1 < coups.Count)
                    {
                        ligne += $"  {coups[i + 1].NotationAlgebrique}";
                        i++; // Sauter le coup suivant
                    }

                    HistoriqueCoups.Add(ligne);
                }
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Démarre une nouvelle partie
        /// </summary>
        private void NouvellePartie()
        {
            var resultat = MessageBox.Show(
                "Voulez-vous vraiment démarrer une nouvelle partie ?",
                "Nouvelle Partie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultat == MessageBoxResult.Yes)
            {
                // Demander les noms des joueurs (simplifié)
                _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);
                ActualiserInfos();
            }
        }

        /// <summary>
        /// Annule le dernier coup
        /// </summary>
        private void AnnulerCoup()
        {
            bool succes = _servicePartie.AnnulerCoup();
            if (succes)
            {
                ActualiserInfos();
            }
        }

        /// <summary>
        /// Propose une partie nulle
        /// </summary>
        private void ProposerNulle()
        {
            var resultat = MessageBox.Show(
                "Proposer une partie nulle ?",
                "Partie Nulle",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultat == MessageBoxResult.Yes)
            {
                _servicePartie.ProposerNulle();

                // Simuler l'acceptation (à améliorer avec un système de confirmation)
                var acceptation = MessageBox.Show(
                    "L'adversaire accepte-t-il la nulle ?",
                    "Acceptation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (acceptation == MessageBoxResult.Yes)
                {
                    _servicePartie.AccepterNulle();
                }
                else
                {
                    _servicePartie.RefuserNulle();
                }

                ActualiserInfos();
            }
        }

        /// <summary>
        /// Abandonne la partie
        /// </summary>
        private void Abandonner()
        {
            var joueurActif = _servicePartie.ObtenirJoueurActif();

            var resultat = MessageBox.Show(
                $"{joueurActif.Nom} abandonne la partie ?",
                "Abandon",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultat == MessageBoxResult.Yes)
            {
                _servicePartie.Abandonner(joueurActif.Couleur);
                ActualiserInfos();
            }
        }

        /// <summary>
        /// Sauvegarde la partie
        /// </summary>
        private void Sauvegarder()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Fichier PGN (*.pgn)|*.pgn|Tous les fichiers (*.*)|*.*",
                DefaultExt = "pgn",
                FileName = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}.pgn"
            };

            if (dialog.ShowDialog() == true)
            {
                bool succes = _servicePartie.SauvegarderPartie(dialog.FileName);

                if (succes)
                {
                    MessageBox.Show("Partie sauvegardée avec succès !", "Sauvegarde",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Erreur lors de la sauvegarde.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Charge une partie
        /// </summary>
        private void Charger()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Fichier PGN (*.pgn)|*.pgn|Tous les fichiers (*.*)|*.*",
                DefaultExt = "pgn"
            };

            if (dialog.ShowDialog() == true)
            {
                bool succes = _servicePartie.ChargerPartie(dialog.FileName);

                if (succes)
                {
                    ActualiserInfos();
                    MessageBox.Show("Partie chargée avec succès !", "Chargement",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Erreur lors du chargement.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Méthodes CanExecute

        private bool PeutAnnulerCoup()
        {
            return _servicePartie.ObtenirHistorique()?.Count > 0;
        }

        private bool PartieEnCours()
        {
            return !_servicePartie.EstPartieTerminee();
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Formate un temps en chaîne lisible
        /// </summary>
        private string FormateurTemps(TimeSpan temps)
        {
            if (temps.TotalHours >= 1)
                return temps.ToString(@"hh\:mm\:ss");
            else
                return temps.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Obtient le texte du statut
        /// </summary>
        private string ObtenirTexteStatut(StatutPartie statut)
        {
            return statut switch
            {
                StatutPartie.EnCours => "Partie en cours",
                StatutPartie.EchecBlanc => "Échec aux blancs !",
                StatutPartie.EchecNoir => "Échec aux noirs !",
                StatutPartie.EchecEtMatBlanc => "Échec et mat ! Les noirs gagnent !",
                StatutPartie.EchecEtMatNoir => "Échec et mat ! Les blancs gagnent !",
                StatutPartie.Pat => "Pat ! Partie nulle",
                StatutPartie.Nulle => "Partie nulle",
                StatutPartie.AbandonBlanc => "Les blancs abandonnent",
                StatutPartie.AbandonNoir => "Les noirs abandonnent",
                _ => "Statut inconnu"
            };
        }

        #endregion
    }
}