using System;
using System.Collections.Generic;
namespace ChessGame.Core.Domain.Models.Pieces
{
    /// <summary>
    /// Représente un pion aux échecs
    /// </summary>
    public class Pion : Piece
    {
        #region Constructeur

        public Pion(CouleurPiece couleur, int ligne, int colonne)
            : base(couleur, ligne, colonne)
        {
            Type = TypePiece.Pion;
            Valeur = 1;
        }

        #endregion

        #region Implémentation des méthodes abstraites

        public override List<Coup> ObtenirCoupsPossibles(Echiquier echiquier)
        {
            List<Coup> coups = new List<Coup>();
            int direction = Couleur == CouleurPiece.Blanc ? -1 : 1; // Blanc monte, Noir descend

            // Avancer d'une case
            int nouvelleLigne = Ligne + direction;
            if (EstPositionValide(nouvelleLigne, Colonne) && EstCaseVide(nouvelleLigne, Colonne, echiquier))
            {
                AjouterCoupSiValide(coups, nouvelleLigne, Colonne, echiquier);

                // Avancer de deux cases si le pion n'a pas encore bougé
                if (!ADejaBougee)
                {
                    int ligneDeux = Ligne + (2 * direction);
                    if (EstPositionValide(ligneDeux, Colonne) && EstCaseVide(ligneDeux, Colonne, echiquier))
                    {
                        AjouterCoupSiValide(coups, ligneDeux, Colonne, echiquier);
                    }
                }
            }

            // Captures diagonales
            int[] colonnesCapture = { Colonne - 1, Colonne + 1 };
            foreach (int col in colonnesCapture)
            {
                if (EstPositionValide(nouvelleLigne, col))
                {
                    // Capture normale
                    if (EstPieceAdverse(nouvelleLigne, col, echiquier))
                    {
                        AjouterCoupSiValide(coups, nouvelleLigne, col, echiquier);
                    }

                    // En passant
                    if (PeutCapturerEnPassant(nouvelleLigne, col, echiquier))
                    {
                        Piece? pionCapture = echiquier.ObtenirPiece(Ligne, col);
                        Coup coupEnPassant = new Coup(this, Ligne, Colonne, nouvelleLigne, col, pionCapture)
                        {
                            EstEnPassant = true
                        };
                        coups.Add(coupEnPassant);
                    }
                }
            }

            return coups;
        }

        public override bool EstCoupValide(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (!EstPositionValide(ligneDestination, colonneDestination))
                return false;

            int direction = Couleur == CouleurPiece.Blanc ? -1 : 1;
            int diffLigne = ligneDestination - Ligne;
            int diffColonne = colonneDestination - Colonne;

            // Avancer tout droit
            if (diffColonne == 0)
            {
                // Une case en avant
                if (diffLigne == direction)
                {
                    return EstCaseVide(ligneDestination, colonneDestination, echiquier);
                }

                // Deux cases en avant (premier mouvement)
                if (diffLigne == 2 * direction && !ADejaBougee)
                {
                    int ligneMilieu = Ligne + direction;
                    return EstCaseVide(ligneMilieu, Colonne, echiquier) &&
                           EstCaseVide(ligneDestination, colonneDestination, echiquier);
                }
            }

            // Capture diagonale
            if (Math.Abs(diffColonne) == 1 && diffLigne == direction)
            {
                return EstPieceAdverse(ligneDestination, colonneDestination, echiquier) ||
                       PeutCapturerEnPassant(ligneDestination, colonneDestination, echiquier);
            }

            return false;
        }

        #endregion

        #region Méthodes spécifiques au pion

        /// <summary>
        /// Vérifie si le pion peut capturer en passant
        /// </summary>
        private bool PeutCapturerEnPassant(int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            // Vérifier si c'est la bonne rangée pour l'en passant
            bool bonneRangee = (Couleur == CouleurPiece.Blanc && Ligne == 3) ||
                               (Couleur == CouleurPiece.Noir && Ligne == 4);

            if (!bonneRangee)
                return false;

            // Vérifier qu'il y a un pion adverse à côté
            Piece? pionAdjacent = echiquier.ObtenirPiece(Ligne, colonneDestination);
            if (pionAdjacent == null || pionAdjacent.Type != TypePiece.Pion ||
                pionAdjacent.Couleur == Couleur)
                return false;

            // Vérifier que le dernier coup était un mouvement de deux cases de ce pion
            Coup? dernierCoup = echiquier.EtatPartie?.DernierCoup;
            if (dernierCoup == null)
                return false;

            return dernierCoup.Piece == pionAdjacent &&
                   Math.Abs(dernierCoup.LigneDepart - dernierCoup.LigneArrivee) == 2 &&
                   dernierCoup.ColonneArrivee == colonneDestination;
        }


        /// <summary>
        /// Vérifie si le pion peut être promu
        /// </summary>
        public bool PeutEtrePromu()
        {
            return (Couleur == CouleurPiece.Blanc && Ligne == 0) ||
                   (Couleur == CouleurPiece.Noir && Ligne == 7);
        }


        #endregion

        #region Clone

        public override Piece Cloner()
        {
            return new Pion(Couleur, Ligne, Colonne)
            {
                ADejaBougee = this.ADejaBougee
            };
        }

        #endregion
    }
}
