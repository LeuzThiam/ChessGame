using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Service de validation des coups d'échecs
    /// </summary>
    public class ValidateurCoup : IValidateurCoup
    {
        #region Validation de base

        /// <summary>
        /// Vérifie si un coup est légal
        /// </summary>
        public bool EstCoupLegal(Coup coup, Echiquier echiquier)
        {
            if (coup == null || coup.Piece == null)
                return false;

            // Vérifier que le mouvement est valide pour la pièce
            if (!EstMouvementValide(coup.Piece, coup.LigneArrivee, coup.ColonneArrivee, echiquier))
                return false;

            // Vérifier que le coup ne met pas son propre roi en échec
            if (CoupMetRoiEnEchec(coup, echiquier))
                return false;

            // Validation spéciale pour le roque
            if (coup.EstPetitRoque || coup.EstGrandRoque)
                return ValiderRoque(coup, echiquier);

            // Validation spéciale pour l'en passant
            if (coup.EstEnPassant)
                return ValiderEnPassant(coup, echiquier);

            return true;
        }

        /// <summary>
        /// Vérifie si un mouvement est valide pour une pièce
        /// </summary>
        public bool EstMouvementValide(Piece piece, int ligneDestination, int colonneDestination, Echiquier echiquier)
        {
            if (piece == null)
                return false;

            // Vérifier que la destination est différente de la position actuelle
            if (piece.Ligne == ligneDestination && piece.Colonne == colonneDestination)
                return false;

            // Déléguer la validation à la pièce elle-même
            return piece.EstCoupValide(ligneDestination, colonneDestination, echiquier);
        }

        #endregion

        #region Coups légaux

        /// <summary>
        /// Obtient tous les coups légaux pour une pièce
        /// </summary>
        public List<Coup> ObtenirCoupsLegaux(Piece piece, Echiquier echiquier)
        {
            if (piece == null)
                return new List<Coup>();

            // Obtenir les coups possibles de la pièce
            List<Coup> coupsPossibles = piece.ObtenirCoupsPossibles(echiquier);

            // Filtrer les coups qui mettraient le roi en échec
            List<Coup> coupsLegaux = new List<Coup>();

            foreach (Coup coup in coupsPossibles)
            {
                if (!CoupMetRoiEnEchec(coup, echiquier))
                {
                    coupsLegaux.Add(coup);
                }
            }

            return coupsLegaux;
        }

        /// <summary>
        /// Obtient tous les coups légaux pour un joueur
        /// </summary>
        public List<Coup> ObtenirTousCoupsLegaux(CouleurPiece couleur, Echiquier echiquier)
        {
            List<Coup> tousLesCoups = new List<Coup>();
            List<Piece> pieces = echiquier.ObtenirPieces(couleur);

            foreach (Piece piece in pieces)
            {
                List<Coup> coupsLegaux = ObtenirCoupsLegaux(piece, echiquier);
                tousLesCoups.AddRange(coupsLegaux);
            }

            return tousLesCoups;
        }

        #endregion

        #region Vérification d'échec

        /// <summary>
        /// Vérifie si un coup mettrait le roi en échec
        /// </summary>
        public bool CoupMetRoiEnEchec(Coup coup, Echiquier echiquier)
        {
            if (coup.Piece == null)
                return true;

            // Créer une copie de l'échiquier
            Echiquier copie = echiquier.Cloner();

            // Exécuter le coup sur la copie
            copie.ExecuterCoup(coup);

            // Vérifier si le roi est en échec
            return copie.EstEnEchec(coup.Piece.Couleur);
        }

        #endregion

        #region Validation des coups spéciaux

        /// <summary>
        /// Valide un roque
        /// </summary>
        public bool ValiderRoque(Coup coup, Echiquier echiquier)
        {
            Roi? roi = coup.Piece as Roi;
            if (roi == null || roi.ADejaBougee)
                return false;

            // Le roi ne doit pas être en échec
            if (echiquier.EstEnEchec(roi.Couleur))
                return false;

            int ligne = roi.Ligne;

            if (coup.EstPetitRoque)
            {
                // Vérifier la tour
                Piece? tourDroite = echiquier.ObtenirPiece(ligne, 7);
                if (tourDroite == null || tourDroite.Type != TypePiece.Tour || tourDroite.ADejaBougee)
                    return false;

                // Vérifier que les cases sont vides
                if (echiquier.ObtenirPiece(ligne, 5) != null || echiquier.ObtenirPiece(ligne, 6) != null)
                    return false;

                // Vérifier que le roi ne passe pas par une case attaquée
                if (echiquier.EstCaseAttaquee(ligne, 5, roi.Couleur) ||
                    echiquier.EstCaseAttaquee(ligne, 6, roi.Couleur))
                    return false;
            }
            else if (coup.EstGrandRoque)
            {
                // Vérifier la tour
                Piece? tourGauche = echiquier.ObtenirPiece(ligne, 0);
                if (tourGauche == null || tourGauche.Type != TypePiece.Tour || tourGauche.ADejaBougee)
                    return false;

                // Vérifier que les cases sont vides
                if (echiquier.ObtenirPiece(ligne, 1) != null ||
                    echiquier.ObtenirPiece(ligne, 2) != null ||
                    echiquier.ObtenirPiece(ligne, 3) != null)
                    return false;

                // Vérifier que le roi ne passe pas par une case attaquée
                if (echiquier.EstCaseAttaquee(ligne, 3, roi.Couleur) ||
                    echiquier.EstCaseAttaquee(ligne, 2, roi.Couleur))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valide une prise en passant
        /// </summary>
        public bool ValiderEnPassant(Coup coup, Echiquier echiquier)
        {
            Pion? pion = coup.Piece as Pion;
            if (pion == null)
                return false;

            // Vérifier que c'est la bonne rangée
            bool bonneRangee = (pion.Couleur == CouleurPiece.Blanc && pion.Ligne == 3) ||
                               (pion.Couleur == CouleurPiece.Noir && pion.Ligne == 4);

            if (!bonneRangee)
                return false;

            // Vérifier qu'il y a un pion adverse à côté
            Piece? pionAdjacent = echiquier.ObtenirPiece(pion.Ligne, coup.ColonneArrivee);
            if (pionAdjacent == null || pionAdjacent.Type != TypePiece.Pion ||
                pionAdjacent.Couleur == pion.Couleur)
                return false;

            // Vérifier que le dernier coup était un mouvement de deux cases de ce pion
            Coup? dernierCoup = echiquier.EtatPartie?.DernierCoup;
            if (dernierCoup == null)
                return false;

            return dernierCoup.Piece == pionAdjacent &&
                   System.Math.Abs(dernierCoup.LigneDepart - dernierCoup.LigneArrivee) == 2 &&
                   dernierCoup.ColonneArrivee == coup.ColonneArrivee;
        }

        internal bool EstCoupValide(Coup coupRoque, Echiquier echiquier)
        {
            if (coupRoque == null || coupRoque.Piece == null)
                return false;

            return EstCoupLegal(coupRoque, echiquier);
        }


        #endregion
    }
}
