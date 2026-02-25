using System;
using System.Collections.Generic;
namespace ChessGame.Core.Domain.Models.Pieces
{
    /// <summary>
    /// Représente une reine aux échecs
    /// La reine combine les mouvements de la tour et du fou
    /// </summary>
    public class Reine : Piece
    {
        #region Constructeur

        public Reine(CouleurPiece couleur, int ligne, int colonne)
            : base(couleur, ligne, colonne)
        {
            Type = TypePiece.Reine;
            Valeur = 9;
        }

        #endregion

        #region Implémentation des méthodes abstraites

        public override List<Coup> ObtenirCoupsPossibles(Echiquier echiquier)
        {
            List<Coup> coups = new List<Coup>();

            // La reine combine les mouvements de la tour (horizontal/vertical)
            // et du fou (diagonal)
            // Directions : 8 directions possibles
            int[,] directions =
            {
                { -1, 0 },  { 1, 0 },   // Vertical (haut, bas)
                { 0, -1 },  { 0, 1 },   // Horizontal (gauche, droite)
                { -1, -1 }, { -1, 1 },  // Diagonales haut
                { 1, -1 },  { 1, 1 }    // Diagonales bas
            };

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int dirLigne = directions[d, 0];
                int dirColonne = directions[d, 1];

                // Explorer dans cette direction jusqu'à rencontrer un obstacle
                for (int i = 1; i < 8; i++)
                {
                    int nouvelleLigne = Ligne + (dirLigne * i);
                    int nouvelleColonne = Colonne + (dirColonne * i);

                    if (!EstPositionValide(nouvelleLigne, nouvelleColonne))
                        break;

                    if (EstCaseVide(nouvelleLigne, nouvelleColonne, echiquier))
                    {
                        AjouterCoupSiValide(coups, nouvelleLigne, nouvelleColonne, echiquier);
                    }
                    else if (EstPieceAdverse(nouvelleLigne, nouvelleColonne, echiquier))
                    {
                        AjouterCoupSiValide(coups, nouvelleLigne, nouvelleColonne, echiquier);
                        break; // Ne peut pas aller plus loin
                    }
                    else
                    {
                        break; // Pièce alliée, on s'arrête
                    }
                }
            }

            return coups;
        }

        public override bool EstCoupValide(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (!EstPositionValide(ligneDestination, colonneDestination))
                return false;

            int diffLigne = Math.Abs(ligneDestination - Ligne);
            int diffColonne = Math.Abs(colonneDestination - Colonne);

            // Mouvement horizontal ou vertical (comme une tour)
            bool mouvementTour = (Ligne == ligneDestination && Colonne != colonneDestination) ||
                                 (Colonne == colonneDestination && Ligne != ligneDestination);

            // Mouvement diagonal (comme un fou)
            bool mouvementFou = diffLigne == diffColonne && diffLigne > 0;

            if (!mouvementTour && !mouvementFou)
                return false;

            // Vérifier que le chemin est libre
            if (!EstCheminLibre(ligneDestination, colonneDestination, echiquier))
                return false;

            // La destination doit être vide ou contenir une pièce adverse
            return EstCaseVide(ligneDestination, colonneDestination, echiquier) ||
                   EstPieceAdverse(ligneDestination, colonneDestination, echiquier);
        }

        #endregion

        #region Méthodes privées

        /// <summary>
        /// Vérifie que le chemin entre la position actuelle et la destination est libre
        /// </summary>
        private bool EstCheminLibre(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            int dirLigne = ligneDestination > Ligne ? 1 : (ligneDestination < Ligne ? -1 : 0);
            int dirColonne = colonneDestination > Colonne ? 1 : (colonneDestination < Colonne ? -1 : 0);

            int ligneCourante = Ligne + dirLigne;
            int colonneCourante = Colonne + dirColonne;

            while (ligneCourante != ligneDestination || colonneCourante != colonneDestination)
            {
                if (!EstCaseVide(ligneCourante, colonneCourante, echiquier))
                    return false;

                ligneCourante += dirLigne;
                colonneCourante += dirColonne;
            }

            return true;
        }

        #endregion

        #region Clone

        public override Piece Cloner()
        {
            return new Reine(Couleur, Ligne, Colonne)
            {
                ADejaBougee = this.ADejaBougee
            };
        }

        #endregion
    }
}