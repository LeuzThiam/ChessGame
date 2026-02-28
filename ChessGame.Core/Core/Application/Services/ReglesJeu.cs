using System;
using System.Collections.Generic;
using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System.Linq;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Service de gestion des règles du jeu d'échecs
    /// </summary>
    public class ReglesJeu : IReglesJeu
    {
        private readonly IValidateurCoup _validateurCoup;

        public ReglesJeu(IValidateurCoup validateurCoup)
        {
            _validateurCoup = validateurCoup;
        }

        #region Échec et Mat

        /// <summary>
        /// Vérifie si un joueur est en échec
        /// </summary>
        public bool EstEnEchec(CouleurPiece couleur, Echiquier echiquier)
        {
            return echiquier.EstEnEchec(couleur);
        }

        /// <summary>
        /// Vérifie si un joueur est en échec et mat
        /// </summary>
        public bool EstEchecEtMat(CouleurPiece couleur, Echiquier echiquier)
        {
            // Le joueur doit être en échec
            if (!EstEnEchec(couleur, echiquier))
                return false;

            // Vérifier s'il existe au moins un coup légal
            List<Coup> coupsLegaux = _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier);
            return coupsLegaux.Count == 0;
        }

        /// <summary>
        /// Vérifie si la partie est en situation de pat
        /// </summary>
        public bool EstPat(CouleurPiece couleur, Echiquier echiquier)
        {
            // Le joueur ne doit PAS être en échec
            if (EstEnEchec(couleur, echiquier))
                return false;

            // Mais il ne doit avoir aucun coup légal
            List<Coup> coupsLegaux = _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier);
            return coupsLegaux.Count == 0;
        }

        #endregion

        #region Nulles

        /// <summary>
        /// Vérifie si la partie est nulle par matériel insuffisant
        /// </summary>
        public bool EstMaterielInsuffisant(Echiquier echiquier)
        {
            List<Piece> piecesBlanches = echiquier.PiecesBlanches;
            List<Piece> piecesNoires = echiquier.PiecesNoires;

            // Roi contre Roi
            if (piecesBlanches.Count == 1 && piecesNoires.Count == 1)
                return true;

            // Roi + Cavalier contre Roi
            if (EstSeulementRoiEtCavalier(piecesBlanches) && piecesNoires.Count == 1)
                return true;
            if (EstSeulementRoiEtCavalier(piecesNoires) && piecesBlanches.Count == 1)
                return true;

            // Roi + Fou contre Roi
            if (EstSeulementRoiEtFou(piecesBlanches) && piecesNoires.Count == 1)
                return true;
            if (EstSeulementRoiEtFou(piecesNoires) && piecesBlanches.Count == 1)
                return true;

            // Roi + Fou contre Roi + Fou (même couleur de case)
            if (EstSeulementRoiEtFou(piecesBlanches) && EstSeulementRoiEtFou(piecesNoires))
            {
                Fou? fouBlanc = piecesBlanches.OfType<Fou>().FirstOrDefault();
                Fou? fouNoir = piecesNoires.OfType<Fou>().FirstOrDefault();
                if (fouBlanc == null || fouNoir == null)
                    return false;

                // Les fous sont sur des cases de même couleur
                bool fouBlancSurCaseClaire = (fouBlanc.Ligne + fouBlanc.Colonne) % 2 == 0;
                bool fouNoirSurCaseClaire = (fouNoir.Ligne + fouNoir.Colonne) % 2 == 0;

                if (fouBlancSurCaseClaire == fouNoirSurCaseClaire)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Vérifie la règle des 50 coups
        /// </summary>
        public bool EstRegleDes50Coups(EtatPartie etatPartie)
        {
            return etatPartie.EstRegleDes50Coups();
        }

        /// <summary>
        /// Vérifie la répétition triple de position
        /// </summary>
        public bool EstRepetitionTriple(EtatPartie etatPartie, string positionFEN)
        {
            return etatPartie.EstRepetitionTriple(positionFEN);
        }

        #endregion

        #region Cases attaquées

        /// <summary>
        /// Vérifie si une case est attaquée
        /// </summary>
        public bool EstCaseAttaquee(int ligne, int colonne, CouleurPiece couleurDefenseur, Echiquier echiquier)
        {
            return echiquier.EstCaseAttaquee(ligne, colonne, couleurDefenseur);
        }

        #endregion

        #region Statut de la partie

        /// <summary>
        /// Détermine le statut de la partie après un coup
        /// </summary>
        public StatutPartie DeterminerStatutPartie(Echiquier echiquier, EtatPartie etatPartie)
        {
            CouleurPiece couleurJoueurActif = etatPartie.JoueurActif.Couleur;
            CouleurPiece couleurAdverse = couleurJoueurActif == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            // Vérifier échec et mat
            if (EstEchecEtMat(couleurJoueurActif, echiquier))
            {
                return couleurJoueurActif == CouleurPiece.Blanc
                    ? StatutPartie.EchecEtMatBlanc
                    : StatutPartie.EchecEtMatNoir;
            }

            // Vérifier pat
            if (EstPat(couleurJoueurActif, echiquier))
            {
                return StatutPartie.Pat;
            }

            // Vérifier matériel insuffisant
            if (EstMaterielInsuffisant(echiquier))
            {
                return StatutPartie.Nulle;
            }

            // Vérifier règle des 50 coups
            if (EstRegleDes50Coups(etatPartie))
            {
                return StatutPartie.Nulle;
            }

            // Vérifier répétition triple
            string positionFEN = echiquier.VersNotationFEN();
            if (EstRepetitionTriple(etatPartie, positionFEN))
            {
                return StatutPartie.Nulle;
            }

            // Vérifier échec
            if (EstEnEchec(couleurJoueurActif, echiquier))
            {
                return couleurJoueurActif == CouleurPiece.Blanc
                    ? StatutPartie.EchecBlanc
                    : StatutPartie.EchecNoir;
            }

            return StatutPartie.EnCours;
        }

        /// <summary>
        /// Vérifie si la partie est terminée
        /// </summary>
        public bool EstPartieTerminee(StatutPartie statut)
        {
            return statut == StatutPartie.EchecEtMatBlanc ||
                   statut == StatutPartie.EchecEtMatNoir ||
                   statut == StatutPartie.Pat ||
                   statut == StatutPartie.Nulle ||
                   statut == StatutPartie.AbandonBlanc ||
                   statut == StatutPartie.AbandonNoir;
        }

        #endregion

        #region Méthodes privées

        /// <summary>
        /// Vérifie si les pièces sont uniquement un roi et un cavalier
        /// </summary>
        private bool EstSeulementRoiEtCavalier(List<Piece> pieces)
        {
            return pieces.Count == 2 &&
                   pieces.Any(p => p.Type == TypePiece.Roi) &&
                   pieces.Any(p => p.Type == TypePiece.Cavalier);
        }

        /// <summary>
        /// Vérifie si les pièces sont uniquement un roi et un fou
        /// </summary>
        private bool EstSeulementRoiEtFou(List<Piece> pieces)
        {
            return pieces.Count == 2 &&
                   pieces.Any(p => p.Type == TypePiece.Roi) &&
                   pieces.Any(p => p.Type == TypePiece.Fou);
        }

        #endregion
    }
}
