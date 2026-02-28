using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Collections.Generic;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Implémentation de l'évaluation de position avec tables positionnelles (piece-square tables)
    /// </summary>
    public class EvaluationPosition : IEvaluationPosition
    {
        private readonly IValidateurCoup _validateurCoup;

        // Valeurs matérielles des pièces
        private const int VALEUR_PION = 100;
        private const int VALEUR_CAVALIER = 320;
        private const int VALEUR_FOU = 330;
        private const int VALEUR_TOUR = 500;
        private const int VALEUR_REINE = 900;
        private const int VALEUR_ROI = 10000;

        // Tables positionnelles (piece-square tables)
        // Les valeurs positives favorisent certaines positions
        // Format: [ligne, colonne] pour les blancs (inversées pour les noirs)
        
        private static readonly int[,] TABLE_PION = new int[,]
        {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 5,  5, 10, 25, 25, 10,  5,  5 },
            { 0,  0,  0, 20, 20,  0,  0,  0 },
            { 5, -5,-10,  0,  0,-10, -5,  5 },
            { 5, 10, 10,-20,-20, 10, 10,  5 },
            { 0,  0,  0,  0,  0,  0,  0,  0 }
        };

        private static readonly int[,] TABLE_CAVALIER = new int[,]
        {
            {-50,-40,-30,-30,-30,-30,-40,-50 },
            {-40,-20,  0,  0,  0,  0,-20,-40 },
            {-30,  0, 10, 15, 15, 10,  0,-30 },
            {-30,  5, 15, 20, 20, 15,  5,-30 },
            {-30,  0, 15, 20, 20, 15,  0,-30 },
            {-30,  5, 10, 15, 15, 10,  5,-30 },
            {-40,-20,  0,  5,  5,  0,-20,-40 },
            {-50,-40,-30,-30,-30,-30,-40,-50 }
        };

        private static readonly int[,] TABLE_FOU = new int[,]
        {
            {-20,-10,-10,-10,-10,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5, 10, 10,  5,  0,-10 },
            {-10,  5,  5, 10, 10,  5,  5,-10 },
            {-10,  0, 10, 10, 10, 10,  0,-10 },
            {-10, 10, 10, 10, 10, 10, 10,-10 },
            {-10,  5,  0,  0,  0,  0,  5,-10 },
            {-20,-10,-10,-10,-10,-10,-10,-20 }
        };

        private static readonly int[,] TABLE_TOUR = new int[,]
        {
            { 0,  0,  0,  0,  0,  0,  0,  0 },
            { 5, 10, 10, 10, 10, 10, 10,  5 },
            {-5,  0,  0,  0,  0,  0,  0, -5 },
            {-5,  0,  0,  0,  0,  0,  0, -5 },
            {-5,  0,  0,  0,  0,  0,  0, -5 },
            {-5,  0,  0,  0,  0,  0,  0, -5 },
            {-5,  0,  0,  0,  0,  0,  0, -5 },
            { 0,  0,  0,  5,  5,  0,  0,  0 }
        };

        private static readonly int[,] TABLE_REINE = new int[,]
        {
            {-20,-10,-10, -5, -5,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5,  5,  5,  5,  0,-10 },
            { -5,  0,  5,  5,  5,  5,  0, -5 },
            {  0,  0,  5,  5,  5,  5,  0, -5 },
            {-10,  5,  5,  5,  5,  5,  0,-10 },
            {-10,  0,  5,  0,  0,  0,  0,-10 },
            {-20,-10,-10, -5, -5,-10,-10,-20 }
        };

        private static readonly int[,] TABLE_ROI_MILIEU = new int[,]
        {
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-20,-30,-30,-40,-40,-30,-30,-20 },
            {-10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        public EvaluationPosition(IValidateurCoup validateurCoup)
        {
            _validateurCoup = validateurCoup ?? throw new ArgumentNullException(nameof(validateurCoup));
        }

        /// <summary>
        /// Évalue une position complète depuis le point de vue d'une couleur
        /// </summary>
        public int Evaluer(Echiquier echiquier, CouleurPiece couleur)
        {
            if (echiquier == null)
                return 0;

            int score = 0;

            // Évaluation matérielle et positionnelle pour les blancs
            score += EvaluerCouleur(echiquier, CouleurPiece.Blanc);

            // Évaluation matérielle et positionnelle pour les noirs (soustraite)
            score -= EvaluerCouleur(echiquier, CouleurPiece.Noir);

            // Si on évalue pour les noirs, on inverse le score
            if (couleur == CouleurPiece.Noir)
                score = -score;

            // Bonus pour la mobilité
            score += EvaluerMobilite(echiquier, couleur) * 2;

            // Bonus/pénalité pour la sécurité du roi
            score += EvaluerSecuriteRoi(echiquier, couleur);

            // Bonus pour le contrôle du centre
            score += EvaluerControleCentre(echiquier, couleur) * 3;

            return score;
        }

        /// <summary>
        /// Évalue un coup spécifique
        /// </summary>
        public int EvaluerCoup(Coup coup, Echiquier echiquier)
        {
            if (coup == null || echiquier == null)
                return 0;

            int score = 0;
            if (coup.Piece == null)
                return score;

            // Valeur de la pièce capturée
            if (coup.PieceCapturee != null)
            {
                score += ObtenirValeurMaterielle(coup.PieceCapturee.Type);
            }

            // Simuler le coup pour évaluer la position résultante
            Echiquier copie = echiquier.Cloner();
            if (!copie.ExecuterCoup(coup))
                return score; // Si le coup ne peut pas être exécuté, retourner le score actuel

            CouleurPiece couleurAdverse = coup.Piece.Couleur == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            // Bonus si le coup donne échec
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

            // Pénalité si le coup expose la pièce à une capture
            if (EstPieceEnDanger(coup.LigneArrivee, coup.ColonneArrivee, coup.Piece.Couleur, copie))
            {
                score -= coup.Piece.Valeur / 10;
            }

            return score;
        }

        #region Méthodes privées d'évaluation

        private int EvaluerCouleur(Echiquier echiquier, CouleurPiece couleur)
        {
            int score = 0;
            List<Piece> pieces = echiquier.ObtenirPieces(couleur);

            foreach (Piece piece in pieces)
            {
                // Valeur matérielle
                score += ObtenirValeurMaterielle(piece.Type);

                // Valeur positionnelle (les tables sont pour les blancs)
                int ligne = piece.Ligne;
                int colonne = piece.Colonne;

                // Pour les noirs, on inverse la ligne (miroir vertical)
                if (couleur == CouleurPiece.Noir)
                    ligne = 7 - ligne;

                score += ObtenirValeurPositionnelle(piece.Type, ligne, colonne);
            }

            return score;
        }

        private int ObtenirValeurMaterielle(TypePiece type)
        {
            return type switch
            {
                TypePiece.Pion => VALEUR_PION,
                TypePiece.Cavalier => VALEUR_CAVALIER,
                TypePiece.Fou => VALEUR_FOU,
                TypePiece.Tour => VALEUR_TOUR,
                TypePiece.Reine => VALEUR_REINE,
                TypePiece.Roi => VALEUR_ROI,
                _ => 0
            };
        }

        private int ObtenirValeurPositionnelle(TypePiece type, int ligne, int colonne)
        {
            return type switch
            {
                TypePiece.Pion => TABLE_PION[ligne, colonne],
                TypePiece.Cavalier => TABLE_CAVALIER[ligne, colonne],
                TypePiece.Fou => TABLE_FOU[ligne, colonne],
                TypePiece.Tour => TABLE_TOUR[ligne, colonne],
                TypePiece.Reine => TABLE_REINE[ligne, colonne],
                TypePiece.Roi => TABLE_ROI_MILIEU[ligne, colonne],
                _ => 0
            };
        }

        private int EvaluerMobilite(Echiquier echiquier, CouleurPiece couleur)
        {
            int mobilite = _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier).Count;
            CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;
            int mobiliteAdverse = _validateurCoup.ObtenirTousCoupsLegaux(couleurAdverse, echiquier).Count;

            return mobilite - mobiliteAdverse;
        }

        private int EvaluerSecuriteRoi(Echiquier echiquier, CouleurPiece couleur)
        {
            Roi? roi = echiquier.TrouverRoi(couleur);
            if (roi == null)
                return -1000; // Roi perdu = pénalité énorme

            int score = 0;

            // Pénalité si le roi est en échec
            if (echiquier.EstEnEchec(couleur))
            {
                score -= 50;
            }

            // Bonus pour la sécurité autour du roi (cases protégées)
            for (int dl = -1; dl <= 1; dl++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dl == 0 && dc == 0)
                        continue;

                    int ligne = roi.Ligne + dl;
                    int colonne = roi.Colonne + dc;

                    if (echiquier.EstPositionValide(ligne, colonne))
                    {
                        if (!echiquier.EstCaseAttaquee(ligne, colonne, couleur))
                        {
                            score += 5;
                        }
                    }
                }
            }

            return score;
        }

        private int EvaluerControleCentre(Echiquier echiquier, CouleurPiece couleur)
        {
            int score = 0;
            int[] casesCentre = { 3, 4 }; // Cases centrales

            foreach (int ligne in casesCentre)
            {
                foreach (int colonne in casesCentre)
                {
                    if (echiquier.EstCaseAttaquee(ligne, colonne, couleur))
                    {
                        score += 1;
                    }

                    CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;
                    if (echiquier.EstCaseAttaquee(ligne, colonne, couleurAdverse))
                    {
                        score -= 1;
                    }
                }
            }

            return score;
        }

        private bool EstPieceEnDanger(int ligne, int colonne, CouleurPiece couleur, Echiquier echiquier)
        {
            CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;
            return echiquier.EstCaseAttaquee(ligne, colonne, couleurAdverse);
        }

        #endregion
    }
}

