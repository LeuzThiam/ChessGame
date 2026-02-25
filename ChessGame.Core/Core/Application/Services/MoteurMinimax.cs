using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Moteur d'IA utilisant l'algorithme Minimax avec élagage Alpha-Beta
    /// </summary>
    public class MoteurMinimax : IMoteurMinimax
    {
        private readonly IValidateurCoup _validateurCoup;
        private readonly IReglesJeu _reglesJeu;
        private readonly IEvaluationPosition _evaluationPosition;

        // Constantes pour les scores terminaux
        private const int SCORE_MAT = 100000;
        private const int SCORE_PAT = 0;
        private const int SCORE_INFINI = 200000;

        public MoteurMinimax(
            IValidateurCoup validateurCoup,
            IReglesJeu reglesJeu,
            IEvaluationPosition evaluationPosition)
        {
            _validateurCoup = validateurCoup ?? throw new ArgumentNullException(nameof(validateurCoup));
            _reglesJeu = reglesJeu ?? throw new ArgumentNullException(nameof(reglesJeu));
            _evaluationPosition = evaluationPosition ?? throw new ArgumentNullException(nameof(evaluationPosition));
        }

        /// <summary>
        /// Trouve le meilleur coup pour une couleur donnée
        /// </summary>
        public Coup? MeilleurCoup(Echiquier echiquier, EtatPartie etatPartie, CouleurPiece couleur, int profondeur)
        {
            if (echiquier == null || etatPartie == null)
                return null;

            if (profondeur <= 0)
                profondeur = 1;

            // Obtenir tous les coups légaux
            List<Coup> coupsLegaux = _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier);

            Console.WriteLine($"[Minimax] MeilleurCoup appelé: {coupsLegaux.Count} coups légaux, profondeur={profondeur}");

            if (coupsLegaux.Count == 0)
                return null;

            // Si un seul coup, pas besoin de chercher
            if (coupsLegaux.Count == 1)
                return coupsLegaux[0];

            // Trier les coups pour améliorer l'efficacité de l'élagage alpha-beta
            coupsLegaux = TrierCoups(coupsLegaux, echiquier);

            Coup? meilleurCoup = null;
            int meilleurScore = int.MinValue;
            int alpha = -SCORE_INFINI;
            int beta = SCORE_INFINI;

            // Créer une copie de l'état pour la recherche
            EtatPartie etatCopie = etatPartie.Cloner();
            Echiquier echiquierCopie = echiquier.Cloner();
            echiquierCopie.EtatPartie = etatCopie;

            // Créer un joueur temporaire pour la recherche
            Joueur joueur = etatCopie.ObtenirJoueurParCouleur(couleur);
            etatCopie.JoueurActif = joueur;
            joueur.CommencerTour();

            int coupIndex = 0;
            foreach (Coup coup in coupsLegaux)
            {
                coupIndex++;
                if (coupIndex % 5 == 0)
                {
                    Console.WriteLine($"[Minimax] Évaluation du coup {coupIndex}/{coupsLegaux.Count}...");
                }

                // Cloner l'échiquier et l'état pour simuler le coup
                Echiquier echiquierTest = echiquierCopie.Cloner();
                EtatPartie etatTest = etatCopie.Cloner();
                echiquierTest.EtatPartie = etatTest;

                // Exécuter le coup
                if (!echiquierTest.ExecuterCoup(coup))
                    continue;

                // Mettre à jour l'état
                etatTest.AjouterCoup(coup);
                CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;
                etatTest.JoueurActif = etatTest.ObtenirJoueurParCouleur(couleurAdverse);

                // Rechercher récursivement
                int score = Minimax(echiquierTest, etatTest, profondeur - 1, alpha, beta, false, couleurAdverse);

                // Mettre à jour le meilleur coup
                if (score > meilleurScore)
                {
                    meilleurScore = score;
                    meilleurCoup = coup;

                    // Élagage alpha-beta
                    alpha = System.Math.Max(alpha, meilleurScore);
                    if (beta <= alpha)
                        break; // Coupe beta
                }
            }

            Console.WriteLine($"[Minimax] MeilleurCoup terminé: coup choisi = {meilleurCoup != null}");
            return meilleurCoup;
        }

        /// <summary>
        /// Algorithme Minimax avec élagage Alpha-Beta
        /// </summary>
        public int Minimax(Echiquier echiquier, EtatPartie etatPartie, int profondeur, int alpha, int beta, bool maximisant, CouleurPiece couleur)
        {
            if (echiquier == null || etatPartie == null)
                return 0;

            // Vérifier les conditions terminales
            int scoreTerminal = EvaluerPositionTerminale(echiquier, etatPartie, couleur);
            if (scoreTerminal != int.MinValue)
                return scoreTerminal;

            // Si profondeur atteinte, évaluer la position
            if (profondeur <= 0)
            {
                return _evaluationPosition.Evaluer(echiquier, couleur);
            }

            // Obtenir tous les coups légaux
            List<Coup> coupsLegaux = _validateurCoup.ObtenirTousCoupsLegaux(couleur, echiquier);

            if (coupsLegaux.Count == 0)
            {
                // Aucun coup légal = pat ou mat
                if (_reglesJeu.EstEnEchec(couleur, echiquier))
                    return maximisant ? -SCORE_MAT + profondeur : SCORE_MAT - profondeur; // Mat
                else
                    return SCORE_PAT; // Pat
            }

            // Trier les coups pour améliorer l'élagage
            coupsLegaux = TrierCoups(coupsLegaux, echiquier);

            if (maximisant)
            {
                int maxEval = -SCORE_INFINI;
                CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;

                foreach (Coup coup in coupsLegaux)
                {
                    // Cloner pour simuler le coup
                    Echiquier echiquierTest = echiquier.Cloner();
                    EtatPartie etatTest = etatPartie.Cloner();
                    echiquierTest.EtatPartie = etatTest;

                    // Exécuter le coup
                    if (!echiquierTest.ExecuterCoup(coup))
                        continue;

                    etatTest.AjouterCoup(coup);
                    etatTest.JoueurActif = etatTest.ObtenirJoueurParCouleur(couleurAdverse);

                    // Recherche récursive
                    int eval = Minimax(echiquierTest, etatTest, profondeur - 1, alpha, beta, false, couleurAdverse);
                    maxEval = System.Math.Max(maxEval, eval);

                    // Élagage alpha-beta
                    alpha = System.Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // Coupe beta
                }

                return maxEval;
            }
            else
            {
                int minEval = SCORE_INFINI;
                CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;

                foreach (Coup coup in coupsLegaux)
                {
                    // Cloner pour simuler le coup
                    Echiquier echiquierTest = echiquier.Cloner();
                    EtatPartie etatTest = etatPartie.Cloner();
                    echiquierTest.EtatPartie = etatTest;

                    // Exécuter le coup
                    if (!echiquierTest.ExecuterCoup(coup))
                        continue;

                    etatTest.AjouterCoup(coup);
                    etatTest.JoueurActif = etatTest.ObtenirJoueurParCouleur(couleurAdverse);

                    // Recherche récursive
                    int eval = Minimax(echiquierTest, etatTest, profondeur - 1, alpha, beta, true, couleurAdverse);
                    minEval = System.Math.Min(minEval, eval);

                    // Élagage alpha-beta
                    beta = System.Math.Min(beta, eval);
                    if (beta <= alpha)
                        break; // Coupe alpha
                }

                return minEval;
            }
        }

        #region Méthodes privées

        /// <summary>
        /// Évalue une position terminale (mat, pat, nulle)
        /// </summary>
        private int EvaluerPositionTerminale(Echiquier echiquier, EtatPartie etatPartie, CouleurPiece couleur)
        {
            if (etatPartie.EstTerminee)
            {
                switch (etatPartie.Statut)
                {
                    case StatutPartie.EchecEtMatBlanc:
                        return couleur == CouleurPiece.Blanc ? -SCORE_MAT : SCORE_MAT;
                    case StatutPartie.EchecEtMatNoir:
                        return couleur == CouleurPiece.Noir ? -SCORE_MAT : SCORE_MAT;
                    case StatutPartie.Pat:
                    case StatutPartie.Nulle:
                        return SCORE_PAT;
                    default:
                        return SCORE_PAT;
                }
            }

            // Vérifier mat
            if (_reglesJeu.EstEchecEtMat(couleur, echiquier))
                return -SCORE_MAT;

            CouleurPiece couleurAdverse = couleur == CouleurPiece.Blanc ? CouleurPiece.Noir : CouleurPiece.Blanc;
            if (_reglesJeu.EstEchecEtMat(couleurAdverse, echiquier))
                return SCORE_MAT;

            // Vérifier pat
            if (_reglesJeu.EstPat(couleur, echiquier))
                return SCORE_PAT;

            return int.MinValue; // Pas une position terminale
        }

        /// <summary>
        /// Trie les coups pour améliorer l'efficacité de l'élagage alpha-beta
        /// Utilise MVV-LVA (Most Valuable Victim - Least Valuable Attacker)
        /// Optimisé pour éviter les clonages coûteux
        /// </summary>
        private List<Coup> TrierCoups(List<Coup> coups, Echiquier echiquier)
        {
            // Trier selon la valeur du coup (captures d'abord, puis autres)
            // Optimisé : on évite de cloner l'échiquier dans le tri car c'est trop coûteux
            return coups.OrderByDescending(c =>
            {
                int score = 0;

                // MVV-LVA : les captures de pièces précieuses par des pièces moins précieuses sont prioritaires
                if (c.PieceCapturee != null)
                {
                    // Score = valeur de la pièce capturée * 10 - valeur de la pièce attaquante
                    score = c.PieceCapturee.Valeur * 10 - c.Piece.Valeur;
                }

                // Bonus pour les coups qui donnent échec (sans cloner pour éviter la lenteur)
                if (c.DonneEchec)
                    score += 50;

                // Utiliser aussi l'évaluation du moteur d'évaluation (sans cloner)
                // Note: EvaluerCoup ne devrait pas cloner non plus
                try
                {
                    score += _evaluationPosition.EvaluerCoup(c, echiquier);
                }
                catch
                {
                    // Ignorer les erreurs d'évaluation dans le tri
                }

                return score;
            }).ToList();
        }

        #endregion
    }
}

