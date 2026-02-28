using ChessGame.Core.Domain.Models.Pieces;

namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Représente une case sur l'échiquier
    /// </summary>
    public class Case
    {
        #region Propriétés

        /// <summary>
        /// Position ligne (0-7)
        /// </summary>
        public int Ligne { get; set; }

        /// <summary>
        /// Position colonne (0-7)
        /// </summary>
        public int Colonne { get; set; }

        /// <summary>
        /// Pièce présente sur la case (null si vide)
        /// </summary>
        public Piece? Piece { get; set; }

        /// <summary>
        /// Indique si la case est claire (alternance échiquier)
        /// </summary>
        public bool EstClaire => (Ligne + Colonne) % 2 == 0;

        /// <summary>
        /// Notation algébrique de la case (ex: "e4", "a1")
        /// </summary>
        public string NotationAlgebrique => $"{(char)('a' + Colonne)}{8 - Ligne}";

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Case()
        {
            Piece = null;
        }

        /// <summary>
        /// Constructeur avec position
        /// </summary>
        /// <param name="ligne">Ligne (0-7)</param>
        /// <param name="colonne">Colonne (0-7)</param>
        public Case(int ligne, int colonne)
        {
            Ligne = ligne;
            Colonne = colonne;
            Piece = null;
        }

        /// <summary>
        /// Constructeur avec position et pièce
        /// </summary>
        /// <param name="ligne">Ligne (0-7)</param>
        /// <param name="colonne">Colonne (0-7)</param>
        /// <param name="piece">Pièce à placer sur la case</param>
        public Case(int ligne, int colonne, Piece? piece)
        {
            Ligne = ligne;
            Colonne = colonne;
            Piece = piece;
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Vérifie si la case est vide
        /// </summary>
        /// <returns>True si aucune pièce n'est présente</returns>
        public bool EstVide()
        {
            return Piece == null;
        }

        /// <summary>
        /// Place une pièce sur la case
        /// </summary>
        /// <param name="piece">Pièce à placer</param>
        public void PlacerPiece(Piece? piece)
        {
            Piece = piece;
            if (piece != null)
            {
                piece.Ligne = Ligne;
                piece.Colonne = Colonne;
            }
        }

        /// <summary>
        /// Retire la pièce de la case
        /// </summary>
        /// <returns>La pièce retirée</returns>
        public Piece? RetirerPiece()
        {
            Piece? pieceRetiree = Piece;
            Piece = null;
            return pieceRetiree;
        }

        /// <summary>
        /// Vérifie si la case contient une pièce d'une certaine couleur
        /// </summary>
        /// <param name="couleur">Couleur à vérifier</param>
        /// <returns>True si la case contient une pièce de cette couleur</returns>
        public bool ContientPieceDeCouleur(CouleurPiece couleur)
        {
            return Piece != null && Piece.Couleur == couleur;
        }

        /// <summary>
        /// Clone la case
        /// </summary>
        /// <returns>Une copie de la case</returns>
        public Case Cloner()
        {
            return new Case(Ligne, Colonne, Piece?.Cloner());
        }

        #endregion

        #region Override

        public override string ToString()
        {
            if (EstVide())
                return $"Case {NotationAlgebrique} (vide)";
            else
                return $"Case {NotationAlgebrique} ({Piece})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Case autreCase)
            {
                return Ligne == autreCase.Ligne && Colonne == autreCase.Colonne;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Ligne * 8 + Colonne).GetHashCode();
        }

        #endregion
    }
}
