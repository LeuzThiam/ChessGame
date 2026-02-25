using System;
using System.Collections.Generic;
namespace ChessGame.Core.Domain.Models.Pieces
{
    /// <summary>
    /// Représente un roi aux échecs
    /// </summary>
    public class Roi : Piece
    {
        #region Constructeur

        public Roi(CouleurPiece couleur, int ligne, int colonne)
            : base(couleur, ligne, colonne)
        {
            Type = TypePiece.Roi;
            Valeur = 1000; // Valeur infinie car perdre le roi = fin de partie
        }

        #endregion

        #region Implémentation des méthodes abstraites

        public override List<Coup> ObtenirCoupsPossibles(Echiquier echiquier)
        {
            List<Coup> coups = new List<Coup>();

            // Le roi peut se déplacer d'une case dans les 8 directions
            int[,] directions =
            {
                { -1, -1 }, { -1, 0 }, { -1, 1 },  // Haut-gauche, haut, haut-droite
                { 0, -1 },             { 0, 1 },   // Gauche, droite
                { 1, -1 },  { 1, 0 },  { 1, 1 }    // Bas-gauche, bas, bas-droite
            };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int nouvelleLigne = Ligne + directions[i, 0];
                int nouvelleColonne = Colonne + directions[i, 1];

                if (EstPositionValide(nouvelleLigne, nouvelleColonne))
                {
                    // Le roi peut aller sur une case vide ou capturer une pièce adverse
                    if (EstCaseVide(nouvelleLigne, nouvelleColonne, echiquier) ||
                        EstPieceAdverse(nouvelleLigne, nouvelleColonne, echiquier))
                    {
                        AjouterCoupSiValide(coups, nouvelleLigne, nouvelleColonne, echiquier);
                    }
                }
            }

            // Ajouter les coups de roque
            AjouterCoupsRoque(coups, echiquier);

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

            // Mouvement normal d'une case
            if (diffLigne <= 1 && diffColonne <= 1)
                return true;

            // Roque
            if (diffLigne == 0 && diffColonne == 2)
                return PeutRoquer(ligneDestination, colonneDestination, echiquier);

            return false;
        }

        #endregion

        #region Méthodes spécifiques au roi

        /// <summary>
        /// Ajoute les coups de roque possibles
        /// </summary>
        private void AjouterCoupsRoque(List<Coup> coups, Echiquier echiquier)
        {
            if (ADejaBougee)
                return;

            // Le roi ne doit pas être en échec
            if (echiquier.EstEnEchec(Couleur))
                return;

            int ligneTour = Couleur == CouleurPiece.Blanc ? 7 : 0;

            // Petit roque (côté roi)
            if (PeutRoquerPetit(echiquier, ligneTour))
            {
                Coup coupRoque = new Coup(this, Ligne, Colonne, Ligne, Colonne + 2, null)
                {
                    EstPetitRoque = true
                };
                coups.Add(coupRoque);
            }

            // Grand roque (côté dame)
            if (PeutRoquerGrand(echiquier, ligneTour))
            {
                Coup coupRoque = new Coup(this, Ligne, Colonne, Ligne, Colonne - 2, null)
                {
                    EstGrandRoque = true
                };
                coups.Add(coupRoque);
            }
        }

        /// <summary>
        /// Vérifie si le petit roque est possible
        /// </summary>
        private bool PeutRoquerPetit(Echiquier echiquier, int ligneTour)
        {
            // Vérifier la tour
            Piece tourDroite = echiquier.ObtenirPiece(ligneTour, 7);
            if (tourDroite == null || tourDroite.Type != TypePiece.Tour || tourDroite.ADejaBougee)
                return false;

            // Vérifier que les cases entre le roi et la tour sont vides
            if (!EstCaseVide(Ligne, 5, echiquier) || !EstCaseVide(Ligne, 6, echiquier))
                return false;

            // Vérifier que le roi ne passe pas par une case attaquée
            return !echiquier.EstCaseAttaquee(Ligne, 5, Couleur) &&
                   !echiquier.EstCaseAttaquee(Ligne, 6, Couleur);
        }

        /// <summary>
        /// Vérifie si le grand roque est possible
        /// </summary>
        private bool PeutRoquerGrand(Echiquier echiquier, int ligneTour)
        {
            // Vérifier la tour
            Piece tourGauche = echiquier.ObtenirPiece(ligneTour, 0);
            if (tourGauche == null || tourGauche.Type != TypePiece.Tour || tourGauche.ADejaBougee)
                return false;

            // Vérifier que les cases entre le roi et la tour sont vides
            if (!EstCaseVide(Ligne, 1, echiquier) ||
                !EstCaseVide(Ligne, 2, echiquier) ||
                !EstCaseVide(Ligne, 3, echiquier))
                return false;

            // Vérifier que le roi ne passe pas par une case attaquée
            return !echiquier.EstCaseAttaquee(Ligne, 3, Couleur) &&
                   !echiquier.EstCaseAttaquee(Ligne, 2, Couleur);
        }

        /// <summary>
        /// Vérifie si le roi peut effectuer un roque vers la destination
        /// </summary>
        private bool PeutRoquer(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (ADejaBougee || echiquier.EstEnEchec(Couleur))
                return false;

            int ligneTour = Couleur == CouleurPiece.Blanc ? 7 : 0;

            // Petit roque (vers la droite)
            if (colonneDestination == Colonne + 2)
                return PeutRoquerPetit(echiquier, ligneTour);

            // Grand roque (vers la gauche)
            if (colonneDestination == Colonne - 2)
                return PeutRoquerGrand(echiquier, ligneTour);

            return false;
        }

        #endregion

        #region Clone

        public override Piece Cloner()
        {
            return new Roi(Couleur, Ligne, Colonne)
            {
                ADejaBougee = this.ADejaBougee
            };
        }

        #endregion
    }
}