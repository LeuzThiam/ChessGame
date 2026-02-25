using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System.Collections.Generic;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Service de génération des coups possibles
    /// </summary>
    public class GenerateurCoups
    {
        private readonly IValidateurCoup _validateurCoup;

        public GenerateurCoups(IValidateurCoup validateurCoup)
        {
            _validateurCoup = validateurCoup;
        }

        #region Génération de coups

        /// <summary>
        /// Génère tous les coups légaux pour une pièce
        /// </summary>
        public List<Coup> GenererCoupsLegaux(Piece piece, Echiquier echiquier)
        {
            if (piece == null)
                return new List<Coup>();

            return _validateurCoup.ObtenirCoupsLegaux(piece, echiquier);
        }

        /// <summary>
        /// Génère tous les coups légaux pour un joueur
        /// </summary>
        public List<Coup> GenererTousCoupsLegaux(CouleurPiece couleur, Echiquier echiquier)
        {
            return _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier);
        }

        /// <summary>
        /// Génère tous les coups de capture possibles
        /// </summary>
        public List<Coup> GenererCoupsCapture(CouleurPiece couleur, Echiquier echiquier)
        {
            List<Coup> tousCoups = GenererTousCoupsLegaux(couleur, echiquier);
            List<Coup> coupsCapture = new List<Coup>();

            foreach (Coup coup in tousCoups)
            {
                if (coup.EstCapture())
                {
                    coupsCapture.Add(coup);
                }
            }

            return coupsCapture;
        }

        /// <summary>
        /// Génère les coups qui donnent échec
        /// </summary>
        public List<Coup> GenererCoupsDonnantEchec(CouleurPiece couleur, Echiquier echiquier)
        {
            List<Coup> tousCoups = GenererTousCoupsLegaux(couleur, echiquier);
            List<Coup> coupsDonnantEchec = new List<Coup>();

            CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            foreach (Coup coup in tousCoups)
            {
                // Simuler le coup
                Echiquier copie = echiquier.Cloner();
                copie.ExecuterCoup(coup);

                // Vérifier si ça donne échec
                if (copie.EstEnEchec(couleurAdverse))
                {
                    coupsDonnantEchec.Add(coup);
                }
            }

            return coupsDonnantEchec;
        }

        #endregion

        #region Génération de coups spéciaux

        /// <summary>
        /// Génère les coups de roque possibles
        /// </summary>
        public List<Coup> GenererCoupsRoque(CouleurPiece couleur, Echiquier echiquier)
        {
            List<Coup> coupsRoque = new List<Coup>();

            Roi roi = echiquier.TrouverRoi(couleur);
            if (roi == null || roi.ADejaBougee)
                return coupsRoque;

            // Petit roque
            Coup petitRoque = new Coup(roi, roi.Ligne, roi.Colonne, roi.Ligne, roi.Colonne + 2)
            {
                EstPetitRoque = true
            };

            if (_validateurCoup.ValiderRoque(petitRoque, echiquier))
            {
                coupsRoque.Add(petitRoque);
            }

            // Grand roque
            Coup grandRoque = new Coup(roi, roi.Ligne, roi.Colonne, roi.Ligne, roi.Colonne - 2)
            {
                EstGrandRoque = true
            };

            if (_validateurCoup.ValiderRoque(grandRoque, echiquier))
            {
                coupsRoque.Add(grandRoque);
            }

            return coupsRoque;
        }

        /// <summary>
        /// Génère les coups de promotion possibles pour un pion
        /// </summary>
        public List<Coup> GenererCoupsPromotion(Pion pion, Echiquier echiquier)
        {
            List<Coup> coupsPromotion = new List<Coup>();

            if (!pion.PeutEtrePromu())
                return coupsPromotion;

            int lignePromotion = pion.Couleur == CouleurPiece.Blanc ? 0 : 7;
            int direction = pion.Couleur == CouleurPiece.Blanc ? -1 : 1;

            // Promotion en avançant
            int ligneDest = pion.Ligne + direction;
            if (echiquier.ObtenirPiece(ligneDest, pion.Colonne) == null)
            {
                AjouterCoupsPromotionPourDestination(pion, ligneDest, pion.Colonne, coupsPromotion);
            }

            // Promotion en capturant
            int[] colonnesCapture = { pion.Colonne - 1, pion.Colonne + 1 };
            foreach (int col in colonnesCapture)
            {
                if (echiquier.EstPositionValide(ligneDest, col))
                {
                    Piece pieceDestination = echiquier.ObtenirPiece(ligneDest, col);
                    if (pieceDestination != null && pieceDestination.Couleur != pion.Couleur)
                    {
                        AjouterCoupsPromotionPourDestination(pion, ligneDest, col, coupsPromotion, pieceDestination);
                    }
                }
            }

            return coupsPromotion;
        }

        #endregion

        #region Évaluation de coups

        /// <summary>
        /// Évalue la valeur d'un coup (pour l'IA)
        /// </summary>
        public int EvaluerCoup(Coup coup, Echiquier echiquier)
        {
            int score = 0;

            // Valeur de base : valeur de la pièce capturée
            if (coup.PieceCapturee != null)
            {
                score += coup.PieceCapturee.Valeur * 10;
            }

            // Bonus si le coup donne échec
            Echiquier copie = echiquier.Cloner();
            copie.ExecuterCoup(coup);

            CouleurPiece couleurAdverse = coup.Piece.Couleur == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            if (copie.EstEnEchec(couleurAdverse))
            {
                score += 50;
            }

            // Bonus pour les coups de développement (début de partie)
            if (!coup.Piece.ADejaBougee &&
                (coup.Piece.Type == TypePiece.Cavalier || coup.Piece.Type == TypePiece.Fou))
            {
                score += 10;
            }

            // Bonus pour contrôler le centre
            if (EstCaseCentrale(coup.LigneArrivee, coup.ColonneArrivee))
            {
                score += 5;
            }

            // Pénalité si le coup expose la pièce à une capture
            if (EstPieceEnDanger(coup, copie))
            {
                score -= coup.Piece.Valeur * 5;
            }

            return score;
        }

        /// <summary>
        /// Trie les coups par ordre de valeur décroissante
        /// </summary>
        public List<Coup> TrierCoupsParValeur(List<Coup> coups, Echiquier echiquier)
        {
            List<Coup> coupsTries = new List<Coup>(coups);

            coupsTries.Sort((c1, c2) =>
                EvaluerCoup(c2, echiquier).CompareTo(EvaluerCoup(c1, echiquier))
            );

            return coupsTries;
        }

        #endregion

        #region Méthodes privées

        /// <summary>
        /// Ajoute les 4 coups de promotion possibles (Reine, Tour, Fou, Cavalier)
        /// </summary>
        private void AjouterCoupsPromotionPourDestination(Pion pion, int ligne, int colonne,
            List<Coup> coups, Piece pieceCapturee = null)
        {
            TypePiece[] piecesPromotion =
            {
                TypePiece.Reine,
                TypePiece.Tour,
                TypePiece.Fou,
                TypePiece.Cavalier
            };

            foreach (TypePiece type in piecesPromotion)
            {
                Coup coup = new Coup(pion, pion.Ligne, pion.Colonne, ligne, colonne, pieceCapturee)
                {
                    EstPromotion = true,
                    PiecePromotion = type
                };
                coups.Add(coup);
            }
        }

        /// <summary>
        /// Vérifie si une case est dans le centre de l'échiquier
        /// </summary>
        private bool EstCaseCentrale(int ligne, int colonne)
        {
            return ligne >= 3 && ligne <= 4 && colonne >= 3 && colonne <= 4;
        }

        /// <summary>
        /// Vérifie si une pièce est en danger après un coup
        /// </summary>
        private bool EstPieceEnDanger(Coup coup, Echiquier echiquierApres)
        {
            return echiquierApres.EstCaseAttaquee(
                coup.LigneArrivee,
                coup.ColonneArrivee,
                coup.Piece.Couleur
            );
        }

        #endregion
    }
}