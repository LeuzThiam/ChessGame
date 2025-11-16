using ChessGame.Models;
using ChessGame.Services.Interfaces;
using ChessGame.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
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
        /// Historique des coups formaté (1. e4 e5, 2. Cf3 Cc6...)
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
            _servicePartie = servicePartie ?? throw new ArgumentNullException(nameof(servicePartie));

            HistoriqueCoups = new ObservableCollection<string>();

            // Initialiser les commandes
            CommandeNouvellePartie = new RelayCommand(_ => NouvellePartie());
            CommandeAnnulerCoup = new RelayCommand(_ => AnnulerCoup(), _ => PeutAnnulerCoup());
            CommandeProposerNulle = new RelayCommand(_ => ProposerNulle(), _ => PartieEnCours());
            CommandeAbandonner = new RelayCommand(_ => Abandonner(), _ => PartieEnCours());
            CommandeSauvegarder = new RelayCommand(_ => Sauvegarder(), _ => PartieEnCours());
            CommandeCharger = new RelayCommand(_ => Charger());

            // S'abonner aux événements du service pour rester synchro
            _servicePartie.CoupJoue += (s, e) => ActualiserInfos();
            _servicePartie.CoupAnnule += (s, e) => ActualiserInfos();
            _servicePartie.PartieTerminee += (s, e) => ActualiserInfos();
            _servicePartie.StatutPartieChange += (s, e) => ActualiserInfos();

            // Si une partie est déjà en cours dans le service
            ActualiserInfos();
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Actualise toutes les informations affichées
        /// </summary>
        public void ActualiserInfos()
        {
            var etat = _servicePartie.EtatPartie;
            if (etat == null)
                return;

            // Noms des joueurs
            NomJoueurBlanc = etat.JoueurBlanc?.Nom ?? "Joueur Blanc";
            NomJoueurNoir = etat.JoueurNoir?.Nom ?? "Joueur Noir";

            // Temps (si tu veux gérer les horloges plus tard, c'est déjà prêt)
            TempsJoueurBlanc = FormaterTemps(etat.JoueurBlanc?.TempsRestant ?? TimeSpan.Zero);
            TempsJoueurNoir = FormaterTemps(etat.JoueurNoir?.TempsRestant ?? TimeSpan.Zero);

            // Statut
            var statut = _servicePartie.ObtenirStatutPartie();
            StatutPartie = ObtenirTexteStatut(statut);

            // Tour actuel
            var joueurActif = _servicePartie.ObtenirJoueurActif();
            TourActuel = joueurActif != null
                ? $"Tour : {joueurActif.Nom} ({joueurActif.Couleur})"
                : "Partie terminée";

            // Historique
            ActualiserHistorique();
        }

        /// <summary>
        /// Actualise l'historique des coups (en lignes style PGN)
        /// </summary>
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
                    // Coup des blancs
                    string ligne = $"{(i / 2) + 1}. {coups[i].NotationAlgebrique}";

                    // Coup des noirs sur la même ligne si présent
                    if (i + 1 < coups.Count)
                    {
                        ligne += $"   {coups[i + 1].NotationAlgebrique}";
                        i++; // on saute le coup des noirs déjà utilisé
                    }

                    HistoriqueCoups.Add(ligne);
                }
            }
        }

        #endregion

        #region Actions (Commandes)

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
                _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);
                ActualiserInfos();
            }
        }

        /// <summary>
        /// Annule le dernier coup
        /// </summary>
        private void AnnulerCoup()
        {
            if (_servicePartie.AnnulerCoup())
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
                "Proposition de nulle",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultat == MessageBoxResult.Yes)
            {
                _servicePartie.ProposerNulle();

                // Ici on simule la réponse de l'adversaire
                var acceptation = MessageBox.Show(
                    "L'adversaire accepte-t-il la nulle ?",
                    "Réponse à la proposition",
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
        /// Abandonner la partie
        /// </summary>
        private void Abandonner()
        {
            var joueurActif = _servicePartie.ObtenirJoueurActif();
            if (joueurActif == null)
                return;

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
        /// Sauvegarder la partie au format PGN
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
                    MessageBox.Show("Partie sauvegardée avec succès !", "Sauvegarde",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Erreur lors de la sauvegarde.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charger une partie depuis un fichier PGN
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
            var liste = _servicePartie.ObtenirHistorique();
            return liste != null && liste.Count > 0 && !_servicePartie.EstPartieTerminee();
        }

        private bool PartieEnCours()
        {
            return !_servicePartie.EstPartieTerminee();
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Formate un TimeSpan en "mm:ss" ou "hh:mm:ss"
        /// </summary>
        private string FormaterTemps(TimeSpan temps)
        {
            if (temps.TotalHours >= 1)
                return temps.ToString(@"hh\:mm\:ss");
            else
                return temps.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Texte lisible pour le statut de partie
        /// </summary>
        private string ObtenirTexteStatut(StatutPartie statut)
        {
            return statut switch
            {
                Models.StatutPartie.EnCours => "Partie en cours",
                Models.StatutPartie.EchecBlanc => "Échec au roi blanc",
                Models.StatutPartie.EchecNoir => "Échec au roi noir",
                Models.StatutPartie.EchecEtMatBlanc => "Échec et mat ! Les noirs gagnent",
                Models.StatutPartie.EchecEtMatNoir => "Échec et mat ! Les blancs gagnent",
                Models.StatutPartie.Pat => "Pat – partie nulle",
                Models.StatutPartie.Nulle => "Partie nulle",
                Models.StatutPartie.AbandonBlanc => "Les blancs abandonnent",
                Models.StatutPartie.AbandonNoir => "Les noirs abandonnent",
                _ => "Statut inconnu"
            };
        }

        #endregion
    }
}
