using System;
using System.Collections.Generic;
namespace ChessGame.Core.Domain.Models.Pieces
{
    /// <summary>
    /// Classe abstraite de base pour toutes les pièces d'échecs
    /// </summary>
    public abstract class Piece
    {
        #region Propriétés

        /// <summary>
        /// Type de la pièce (Pion, Tour, Cavalier, etc.)
        /// </summary>
        public TypePiece Type { get; protected set; }

        /// <summary>
        /// Couleur de la pièce (Blanc ou Noir)
        /// </summary>
        public CouleurPiece Couleur { get; protected set; }

        /// <summary>
        /// Position actuelle sur l'échiquier (ligne)
        /// </summary>
        public int Ligne { get; set; }

        /// <summary>
        /// Position actuelle sur l'échiquier (colonne)
        /// </summary>
        public int Colonne { get; set; }

        /// <summary>
        /// Indique si la pièce a déjà bougé (important pour le roque et le pion)
        /// </summary>
        public bool ADejaBougee { get; set; }

        /// <summary>
        /// Valeur de la pièce (pour l'évaluation)
        /// </summary>
        public int Valeur { get; protected set; }

        #endregion

        #region Constructeur

        /// <summary>
        /// Constructeur de base pour une pièce
        /// </summary>
        /// <param name="couleur">Couleur de la pièce</param>
        /// <param name="ligne">Position ligne</param>
        /// <param name="colonne">Position colonne</param>
        protected Piece(CouleurPiece couleur, int ligne, int colonne)
        {
            Couleur = couleur;
            Ligne = ligne;
            Colonne = colonne;
            ADejaBougee = false;
        }

        #endregion

        #region Méthodes abstraites

        /// <summary>
        /// Retourne tous les coups possibles pour cette pièce
        /// </summary>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>Liste des coups possibles</returns>
        public abstract List<Coup> ObtenirCoupsPossibles(Echiquier echiquier);

        /// <summary>
        /// Vérifie si un coup est valide pour cette pièce
        /// </summary>
        /// <param name="ligneDestination">Ligne de destination</param>
        /// <param name="colonneDestination">Colonne de destination</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le coup est valide</returns>
        public abstract bool EstCoupValide(int ligneDestination, int colonneDestination, Echiquier echiquier);

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Clone la pièce (pour simuler des coups)
        /// </summary>
        /// <returns>Une copie de la pièce</returns>
        public abstract Piece Cloner();

        /// <summary>
        /// Vérifie si une position est valide sur l'échiquier
        /// </summary>
        /// <param name="ligne">Ligne à vérifier</param>
        /// <param name="colonne">Colonne à vérifier</param>
        /// <returns>True si la position est valide</returns>
        protected bool EstPositionValide(int ligne, int colonne)
        {
            return ligne >= 0 && ligne < 8 && colonne >= 0 && colonne < 8;
        }

        /// <summary>
        /// Vérifie si une case est vide
        /// </summary>
        protected bool EstCaseVide(int ligne, int colonne, Echiquier echiquier)
        {
            return echiquier.ObtenirPiece(ligne, colonne) == null;
        }

        /// <summary>
        /// Vérifie si une case contient une pièce adverse
        /// </summary>
        protected bool EstPieceAdverse(int ligne, int colonne, Echiquier echiquier)
        {
            Piece? piece = echiquier.ObtenirPiece(ligne, colonne);
            return piece != null && piece.Couleur != this.Couleur;
        }

        /// <summary>
        /// Vérifie si une case contient une pièce alliée
        /// </summary>
        protected bool EstPieceAlliee(int ligne, int colonne, Echiquier echiquier)
        {
            Piece? piece = echiquier.ObtenirPiece(ligne, colonne);
            return piece != null && piece.Couleur == this.Couleur;
        }

        /// <summary>
        /// Ajoute un coup à la liste si la destination est valide
        /// </summary>
        protected void AjouterCoupSiValide(List<Coup> coups, int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (EstCoupValide(ligneDestination, colonneDestination, echiquier))
            {
                Piece? pieceCapturee = echiquier.ObtenirPiece(ligneDestination, colonneDestination);
                coups.Add(new Coup(this, Ligne, Colonne, ligneDestination, colonneDestination, pieceCapturee));
            }
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return $"{Couleur} {Type} en ({Ligne}, {Colonne})";
        }

        #endregion
    }
}
