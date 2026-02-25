
using System;
using System.Collections.Generic;

namespace ChessGame.Core.Domain.Models.Pieces
{
    /// <summary>
    /// Représente un cavalier aux échecs
    /// </summary>
    public class Cavalier : Piece
    {
        #region Constructeur

        public Cavalier(CouleurPiece couleur, int ligne, int colonne)
            : base(couleur, ligne, colonne)
        {
            Type = TypePiece.Cavalier;
            Valeur = 3;
        }

        #endregion

        #region Implémentation des méthodes abstraites

        public override List<Coup> ObtenirCoupsPossibles(Echiquier echiquier)
        {
            List<Coup> coups = new List<Coup>();

            // Les 8 mouvements possibles du cavalier en forme de "L"
            int[,] deplacements =
            {
                { -2, -1 }, { -2, 1 },  // Haut-gauche et haut-droite
                { -1, -2 }, { -1, 2 },  // Gauche-haut et droite-haut
                { 1, -2 },  { 1, 2 },   // Gauche-bas et droite-bas
                { 2, -1 },  { 2, 1 }    // Bas-gauche et bas-droite
            };

            for (int i = 0; i < deplacements.GetLength(0); i++)
            {
                int nouvelleLigne = Ligne + deplacements[i, 0];
                int nouvelleColonne = Colonne + deplacements[i, 1];

                if (EstPositionValide(nouvelleLigne, nouvelleColonne))
                {
                    // Le cavalier peut aller sur une case vide ou capturer une pièce adverse
                    if (EstCaseVide(nouvelleLigne, nouvelleColonne, echiquier) ||
                        EstPieceAdverse(nouvelleLigne, nouvelleColonne, echiquier))
                    {
                        AjouterCoupSiValide(coups, nouvelleLigne, nouvelleColonne, echiquier);
                    }
                }
            }

            return coups;
        }

        public override bool EstCoupValide(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (!EstPositionValide(ligneDestination, colonneDestination))
                return false;

            // Ne peut pas capturer ses propres pièces
            if (EstPieceAlliee(ligneDestination, colonneDestination, echiquier))
                return false;

            int diffLigne = Math.Abs(ligneDestination - Ligne);
            int diffColonne = Math.Abs(colonneDestination - Colonne);

            // Mouvement en L : (2,1) ou (1,2)
            return (diffLigne == 2 && diffColonne == 1) || (diffLigne == 1 && diffColonne == 2);
        }

        #endregion

        #region Clone

        public override Piece Cloner()
        {
            return new Cavalier(Couleur, Ligne, Colonne)
            {
                ADejaBougee = this.ADejaBougee
            };
        }

        #endregion
    }
}