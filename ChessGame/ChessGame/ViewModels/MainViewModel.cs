using ChessGame.Models;
using ChessGame.Services;
using ChessGame.Services.Interfaces;
using ChessGame.ViewModels.Base;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// ViewModel principal de l'application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IServicePartie _servicePartie;
        private string _messageStatut;
        private bool _afficherCoordonnees;
        private bool _afficherCoupsPossibles;

        #region Propriétés

        /// <summary>
        /// ViewModel de l'échiquier
        /// </summary>
        public EchiquierViewModel EchiquierViewModel { get; }

        /// <summary>
        /// ViewModel du panneau d'informations
        /// </summary>
        public InfoPartieViewModel InfoPartieViewModel { get; }

        /// <summary>
        /// Message de statut affiché en haut
        /// </summary>
        public string MessageStatut
        {
            get => _messageStatut;
            set => SetProperty(ref _messageStatut, value);
        }

        /// <summary>
        /// Afficher les coordonnées de l'échiquier
        /// </summary>
        public bool AfficherCoordonnees
        {
            get => _afficherCoordonnees;
            set => SetProperty(ref _afficherCoordonnees, value);
        }

        /// <summary>
        /// Afficher les coups possibles
        /// </summary>
        public bool AfficherCoupsPossibles
        {
            get => _afficherCoupsPossibles;
            set => SetProperty(ref _afficherCoupsPossibles, value);
        }

        #endregion

        #region Commandes

        public ICommand CommandeNouvellePartie { get; }
        public ICommand CommandeChargerPartie { get; }
        public ICommand CommandeSauvegarderPartie { get; }
        public ICommand CommandeQuitter { get; }
        public ICommand CommandeAnnulerCoup { get; }
        public ICommand CommandeRefaireCoup { get; }
        public ICommand CommandeProposerNulle { get; }
        public ICommand CommandeAbandonner { get; }
        public ICommand CommandeRotationEchiquier { get; }
        public ICommand CommandeAfficherRegles { get; }
        public ICommand CommandeAPropos { get; }

        #endregion

        #region Constructeur

        public MainViewModel()
        {
            // Créer le service de partie
            _servicePartie = new ServicePartie();

            // Créer les ViewModels enfants
            EchiquierViewModel = new EchiquierViewModel(_servicePartie);
            InfoPartieViewModel = new InfoPartieViewModel(_servicePartie);

            // Initialiser les propriétés
            AfficherCoordonnees = true;
            AfficherCoupsPossibles = true;
            MessageStatut = "Bienvenue ! Démarrez une nouvelle partie.";

            // Initialiser les commandes
            CommandeNouvellePartie = new RelayCommand(_ => NouvellePartie());
            CommandeChargerPartie = new RelayCommand(_ => ChargerPartie());
            CommandeSauvegarderPartie = new RelayCommand(_ => SauvegarderPartie());
            CommandeQuitter = new RelayCommand(_ => Quitter());
            CommandeAnnulerCoup = new RelayCommand(_ => AnnulerCoup(), _ => PeutAnnulerCoup());
            CommandeRefaireCoup = new RelayCommand(_ => RefaireCoup(), _ => PeutRefaireCoup());
            CommandeProposerNulle = new RelayCommand(_ => ProposerNulle());
            CommandeAbandonner = new RelayCommand(_ => Abandonner());
            CommandeRotationEchiquier = new RelayCommand(_ => RotationEchiquier());
            CommandeAfficherRegles = new RelayCommand(_ => AfficherRegles());
            CommandeAPropos = new RelayCommand(_ => APropos());

            // S'abonner aux événements
            _servicePartie.CoupJoue += (s, e) => ActualiserMessageStatut();
            _servicePartie.PartieTerminee += (s, statut) => AfficherFinPartie(statut);
            _servicePartie.JoueurEnEchec += (s, couleur) => MessageStatut = $"{couleur} est en échec !";

            // Démarrer une partie par défaut
            DemarrerPartieDefaut();
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Démarre une partie avec configuration par défaut
        /// </summary>
        private void DemarrerPartieDefaut()
        {
            _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);
            EchiquierViewModel.ActualiserEchiquier();
            InfoPartieViewModel.ActualiserInfos();
            MessageStatut = "Nouvelle partie démarrée. Les blancs commencent.";
        }

        #endregion

        #region Actions

        /// <summary>
        /// Démarre une nouvelle partie
        /// </summary>
        private void NouvellePartie()
        {
            var resultat = MessageBox.Show(
                "Voulez-vous démarrer une nouvelle partie ?",
                "Nouvelle Partie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultat == MessageBoxResult.Yes)
            {
                // TODO: Afficher une boîte de dialogue pour configurer la partie
                _servicePartie.DemarrerNouvellePartie("Joueur Blanc", "Joueur Noir", 10);
                EchiquierViewModel.ReinitialiserEchiquier();
                InfoPartieViewModel.ActualiserInfos();
                MessageStatut = "Nouvelle partie démarrée. Les blancs commencent.";
            }
        }

        /// <summary>
        /// Charge une partie depuis un fichier
        /// </summary>
        private void ChargerPartie()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Fichiers PGN (*.pgn)|*.pgn|Tous les fichiers (*.*)|*.*",
                Title = "Charger une partie"
            };

            if (dialog.ShowDialog() == true)
            {
                bool succes = _servicePartie.ChargerPartie(dialog.FileName);

                if (succes)
                {
                    EchiquierViewModel.ActualiserEchiquier();
                    InfoPartieViewModel.ActualiserInfos();
                    MessageStatut = "Partie chargée avec succès.";
                }
                else
                {
                    MessageBox.Show("Erreur lors du chargement de la partie.", "Erreur",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Sauvegarde la partie actuelle
        /// </summary>
        private void SauvegarderPartie()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Fichiers PGN (*.pgn)|*.pgn",
                DefaultExt = "pgn",
                FileName = $"Partie_{DateTime.Now:yyyyMMdd_HHmmss}.pgn",
                Title = "Sauvegarder la partie"
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
        /// Quitte l'application
        /// </summary>
        private void Quitter()
        {
            var resultat = MessageBox.Show(
                "Voulez-vous vraiment quitter ?",
                "Quitter",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultat == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
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
                EchiquierViewModel.ActualiserEchiquier();
                InfoPartieViewModel.ActualiserInfos();
                MessageStatut = "Coup annulé.";
            }
        }

        /// <summary>
        /// Refait un coup annulé
        /// </summary>
        private void RefaireCoup()
        {
            bool succes = _servicePartie.RefaireCoup();
            if (succes)
            {
                EchiquierViewModel.ActualiserEchiquier();
                InfoPartieViewModel.ActualiserInfos();
                MessageStatut = "Coup refait.";
            }
        }

        /// <summary>
        /// Propose une partie nulle
        /// </summary>
        private void ProposerNulle()
        {
            InfoPartieViewModel.CommandeProposerNulle.Execute(null);
        }

        /// <summary>
        /// Abandonne la partie
        /// </summary>
        private void Abandonner()
        {
            InfoPartieViewModel.CommandeAbandonner.Execute(null);
        }

        /// <summary>
        /// Rotation de l'échiquier (vue inverse)
        /// </summary>
        private void RotationEchiquier()
        {
            // TODO: Implémenter la rotation de l'échiquier
            MessageBox.Show("Fonctionnalité de rotation à venir !", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Affiche les règles du jeu
        /// </summary>
        private void AfficherRegles()
        {
            string regles = @"RÈGLES DES ÉCHECS

Le but du jeu est de mettre le roi adverse échec et mat.

MOUVEMENT DES PIÈCES:
♙ Pion: Avance d'une case, deux cases au premier coup. Capture en diagonale.
♜ Tour: Se déplace horizontalement ou verticalement.
♞ Cavalier: Se déplace en L (2+1 cases).
♝ Fou: Se déplace en diagonale.
♛ Reine: Combine tour et fou.
♔ Roi: Une case dans toutes les directions.

RÈGLES SPÉCIALES:
- Roque: Roi + Tour (conditions spécifiques)
- En passant: Capture spéciale du pion
- Promotion: Le pion devient une autre pièce en atteignant la dernière rangée

Bonne partie !";

            MessageBox.Show(regles, "Règles du Jeu",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Affiche les informations À propos
        /// </summary>
        private void APropos()
        {
            string info = @"JEU D'ÉCHECS
Version 1.0

Développé en C# WPF avec pattern MVVM

© 2025 - Tous droits réservés

Profitez de votre partie d'échecs !";

            MessageBox.Show(info, "À Propos",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region CanExecute

        private bool PeutAnnulerCoup()
        {
            return _servicePartie.ObtenirHistorique()?.Count > 0;
        }

        private bool PeutRefaireCoup()
        {
            // TODO: Implémenter la logique de refaire
            return false;
        }

        #endregion

        #region Mise à jour

        /// <summary>
        /// Actualise le message de statut
        /// </summary>
        private void ActualiserMessageStatut()
        {
            var joueurActif = _servicePartie.ObtenirJoueurActif();
            if (joueurActif != null)
            {
                MessageStatut = $"Tour de {joueurActif.Nom} ({joueurActif.Couleur})";
            }
        }

        /// <summary>
        /// Affiche le message de fin de partie
        /// </summary>
        private void AfficherFinPartie(StatutPartie statut)
        {
            string message = statut switch
            {
                StatutPartie.EchecEtMatBlanc => "Échec et mat ! Les noirs gagnent !",
                StatutPartie.EchecEtMatNoir => "Échec et mat ! Les blancs gagnent !",
                StatutPartie.Pat => "Pat ! Partie nulle.",
                StatutPartie.Nulle => "Partie nulle.",
                StatutPartie.AbandonBlanc => "Les blancs abandonnent. Les noirs gagnent !",
                StatutPartie.AbandonNoir => "Les noirs abandonnent. Les blancs gagnent !",
                _ => "Partie terminée."
            };

            MessageStatut = message;

            MessageBox.Show(message, "Partie Terminée",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}