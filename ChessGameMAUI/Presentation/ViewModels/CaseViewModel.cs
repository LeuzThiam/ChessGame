using ChessGame.Core.Domain.Models.Pieces;
using ChessGameMAUI.ViewModels.Base;
using System.Windows.Input;

namespace ChessGameMAUI.ViewModels
{
    /// <summary>
    /// ViewModel pour une case de l'échiquier
    /// </summary>
    public class CaseViewModel : ViewModelBase
    {
        private Piece? _piece;
        private bool _estSelectionnee;
        private bool _estCoupPossible;
        private bool _estDernierCoup;
        private bool _estEnEchec;

        #region Propriétés

        /// <summary>
        /// Ligne de la case (0-7)
        /// </summary>
        public int Ligne { get; set; }

        /// <summary>
        /// Colonne de la case (0-7)
        /// </summary>
        public int Colonne { get; set; }

        /// <summary>
        /// Pièce présente sur la case
        /// </summary>
        public Piece? Piece
        {
            get => _piece;
            set => SetProperty(ref _piece, value);
        }

        /// <summary>
        /// Indique si la case est sélectionnée
        /// </summary>
        public bool EstSelectionnee
        {
            get => _estSelectionnee;
            set
            {
                if (SetProperty(ref _estSelectionnee, value))
                {
                    OnPropertyChanged(nameof(CouleurFond));
                }
            }
        }

        /// <summary>
        /// Indique si la case est un coup possible
        /// </summary>
        public bool EstCoupPossible
        {
            get => _estCoupPossible;
            set
            {
                if (SetProperty(ref _estCoupPossible, value))
                {
                    OnPropertyChanged(nameof(CouleurFond));
                    OnPropertyChanged(nameof(AfficherIndicateurCoup));
                }
            }
        }

        /// <summary>
        /// Indique si la case fait partie du dernier coup
        /// </summary>
        public bool EstDernierCoup
        {
            get => _estDernierCoup;
            set
            {
                if (SetProperty(ref _estDernierCoup, value))
                {
                    OnPropertyChanged(nameof(CouleurFond));
                }
            }
        }

        /// <summary>
        /// Indique si un roi en échec est sur cette case
        /// </summary>
        public bool EstEnEchec
        {
            get => _estEnEchec;
            set
            {
                if (SetProperty(ref _estEnEchec, value))
                {
                    OnPropertyChanged(nameof(CouleurFond));
                }
            }
        }

        /// <summary>
        /// Indique si la case est claire (selon l'alternance de l'échiquier)
        /// </summary>
        public bool EstClaire => (Ligne + Colonne) % 2 == 0;

        /// <summary>
        /// Couleur de fond de la case (selon son état)
        /// </summary>
        public Brush CouleurFond
        {
            get
            {
                if (EstEnEchec)
                    return new SolidColorBrush(Color.FromRgb(255, 107, 107)); // Rouge pour échec

                if (EstSelectionnee)
                    return new SolidColorBrush(Color.FromRgb(255, 255, 0)); // Jaune pour sélection

                if (EstDernierCoup)
                    return new SolidColorBrush(Color.FromRgb(255, 224, 130)); // Orange clair

                if (EstCoupPossible)
                    return new SolidColorBrush(Color.FromRgb(144, 238, 144)); // Vert clair

                // Couleurs normales de l'échiquier
                if (EstClaire)
                    return new SolidColorBrush(Color.FromRgb(240, 217, 181)); // Beige clair
                else
                    return new SolidColorBrush(Color.FromRgb(181, 136, 99)); // Marron
            }
        }

        /// <summary>
        /// Indique s'il faut afficher l'indicateur de coup possible
        /// </summary>
        public bool AfficherIndicateurCoup => EstCoupPossible;

        /// <summary>
        /// Notation algébrique de la case (ex: "e4")
        /// </summary>
        public string NotationAlgebrique => $"{(char)('a' + Colonne)}{8 - Ligne}";

        #endregion

        #region Commandes

        /// <summary>
        /// Commande exécutée lors du clic sur la case
        /// </summary>
        public ICommand CommandeClic { get; set; }

        #endregion

        #region Constructeur

        public CaseViewModel(int ligne, int colonne)
        {
            Ligne = ligne;
            Colonne = colonne;
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Réinitialise l'état visuel de la case
        /// </summary>
        public void ReinitialiserEtat()
        {
            EstSelectionnee = false;
            EstCoupPossible = false;
            EstDernierCoup = false;
            EstEnEchec = false;
        }

        public override string ToString()
        {
            return $"Case {NotationAlgebrique} - {(Piece != null ? Piece.ToString() : "Vide")}";
        }

        #endregion
    }
}