using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using ChessGameMAUI.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChessGameMAUI.ViewModels
{
    /// <summary>
    /// ViewModel pour l'échiquier complet
    /// </summary>
    public class EchiquierViewModel : ViewModelBase
    {
        private readonly IServicePartie _servicePartie;
        private CaseViewModel _caseSelectionnee;
        private List<Coup> _coupsPossibles;

        #region Propriétés

        /// <summary>
        /// Collection des 64 cases de l'échiquier
        /// </summary>
        public ObservableCollection<CaseViewModel> Cases { get; }

        /// <summary>
        /// Coordonnées des colonnes (a-h)
        /// </summary>
        public ObservableCollection<string> CoordonneesColonnes { get; }

        /// <summary>
        /// Coordonnées des lignes (8-1)
        /// </summary>
        public ObservableCollection<string> CoordonneesLignes { get; }

        #endregion

        #region Constructeur

        public EchiquierViewModel(IServicePartie servicePartie)
        {
            _servicePartie = servicePartie;

            // Initialiser les collections
            Cases = new ObservableCollection<CaseViewModel>();
            CoordonneesColonnes = new ObservableCollection<string> { "a", "b", "c", "d", "e", "f", "g", "h" };
            CoordonneesLignes = new ObservableCollection<string> { "8", "7", "6", "5", "4", "3", "2", "1" };

            // Créer les 64 cases
            InitialiserCases();
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise les 64 cases de l'échiquier
        /// </summary>
        private void InitialiserCases()
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    var caseVM = new CaseViewModel(ligne, colonne);
                    caseVM.CommandeClic = new RelayCommand(param => GererClicCase(caseVM));
                    Cases.Add(caseVM);
                }
            }
        }

        #endregion

        #region Gestion des clics

        /// <summary>
        /// Gère le clic sur une case
        /// </summary>
        private void GererClicCase(CaseViewModel caseCliquee)
        {
            if (_caseSelectionnee == null)
            {
                // Première sélection : sélectionner une pièce
                SelectionnerCase(caseCliquee);
            }
            else
            {
                // Deuxième clic : tenter de jouer un coup
                if (caseCliquee == _caseSelectionnee)
                {
                    // Clic sur la même case : désélectionner
                    DeselectionnerCase();
                }
                else if (caseCliquee.EstCoupPossible)
                {
                    // Jouer le coup
                    JouerCoup(_caseSelectionnee, caseCliquee);
                }
                else if (caseCliquee.Piece != null &&
                         caseCliquee.Piece.Couleur == _servicePartie.EtatPartie.JoueurActif.Couleur)
                {
                    // Sélectionner une autre pièce du même joueur
                    DeselectionnerCase();
                    SelectionnerCase(caseCliquee);
                }
                else
                {
                    // Clic invalide : désélectionner
                    DeselectionnerCase();
                }
            }
        }

        /// <summary>
        /// Sélectionne une case et affiche les coups possibles
        /// </summary>
        private void SelectionnerCase(CaseViewModel caseVM)
        {
            if (caseVM.Piece == null)
                return;

            // Vérifier que c'est le bon joueur
            if (caseVM.Piece.Couleur != _servicePartie.EtatPartie.JoueurActif.Couleur)
                return;

            _caseSelectionnee = caseVM;
            caseVM.EstSelectionnee = true;

            // Obtenir et afficher les coups possibles
            _coupsPossibles = _servicePartie.ObtenirCoupsPossibles(caseVM.Ligne, caseVM.Colonne);

            foreach (var coup in _coupsPossibles)
            {
                var caseDestination = ObtenirCase(coup.LigneArrivee, coup.ColonneArrivee);
                if (caseDestination != null)
                {
                    caseDestination.EstCoupPossible = true;
                }
            }
        }

        /// <summary>
        /// Désélectionne la case actuelle
        /// </summary>
        private void DeselectionnerCase()
        {
            if (_caseSelectionnee != null)
            {
                _caseSelectionnee.EstSelectionnee = false;
                _caseSelectionnee = null;
            }

            // Effacer les coups possibles
            foreach (var caseVM in Cases)
            {
                caseVM.EstCoupPossible = false;
            }

            _coupsPossibles = null;
        }

        /// <summary>
        /// Joue un coup sur l'échiquier
        /// </summary>
        private void JouerCoup(CaseViewModel caseDepart, CaseViewModel caseArrivee)
        {
            bool succes = _servicePartie.JouerCoup(
                caseDepart.Ligne,
                caseDepart.Colonne,
                caseArrivee.Ligne,
                caseArrivee.Colonne);

            if (succes)
            {
                // Mettre à jour l'affichage
                ActualiserEchiquier();
                DeselectionnerCase();
            }
        }

        #endregion

        #region Mise à jour de l'affichage

        /// <summary>
        /// Actualise l'affichage complet de l'échiquier
        /// </summary>
        public void ActualiserEchiquier()
        {
            // Réinitialiser les états visuels
            foreach (var caseVM in Cases)
            {
                caseVM.ReinitialiserEtat();
            }

            // Mettre à jour les pièces
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    var caseVM = ObtenirCase(ligne, colonne);
                    if (caseVM != null)
                    {
                        caseVM.Piece = _servicePartie.ObtenirPiece(ligne, colonne);
                    }
                }
            }

            // Mettre en évidence le dernier coup
            if (_servicePartie.EtatPartie.DernierCoup != null)
            {
                var dernierCoup = _servicePartie.EtatPartie.DernierCoup;

                var caseDepart = ObtenirCase(dernierCoup.LigneDepart, dernierCoup.ColonneDepart);
                var caseArrivee = ObtenirCase(dernierCoup.LigneArrivee, dernierCoup.ColonneArrivee);

                if (caseDepart != null) caseDepart.EstDernierCoup = true;
                if (caseArrivee != null) caseArrivee.EstDernierCoup = true;
            }

            // Mettre en évidence le roi en échec
            var couleurActive = _servicePartie.EtatPartie.JoueurActif.Couleur;
            if (_servicePartie.EstEnEchec(couleurActive))
            {
                var roi = _servicePartie.Echiquier.TrouverRoi(couleurActive);
                if (roi != null)
                {
                    var caseRoi = ObtenirCase(roi.Ligne, roi.Colonne);
                    if (caseRoi != null)
                    {
                        caseRoi.EstEnEchec = true;
                    }
                }
            }
        }

        /// <summary>
        /// Réinitialise l'échiquier pour une nouvelle partie
        /// </summary>
        public void ReinitialiserEchiquier()
        {
            DeselectionnerCase();
            ActualiserEchiquier();
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Obtient une case spécifique
        /// </summary>
        private CaseViewModel ObtenirCase(int ligne, int colonne)
        {
            int index = ligne * 8 + colonne;
            if (index >= 0 && index < Cases.Count)
            {
                return Cases[index];
            }
            return null;
        }

        #endregion
    }
}