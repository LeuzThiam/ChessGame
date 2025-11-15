using ChessGame.Models;
using ChessGame.Models.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame.Utilitaires
{
    /// <summary>
    /// Utilitaires pour manipuler et analyser l'échiquier
    /// </summary>
    public static class UtilitaireEchiquier
    {
        #region Validation de positions

        /// <summary>
        /// Vérifie si une position est valide sur l'échiquier
        /// </summary>
        /// <param name="ligne">Ligne (0-7)</param>
        /// <param name="colonne">Colonne (0-7)</param>
        /// <returns>True si la position est sur l'échiquier</returns>
        public static bool EstPositionValide(int ligne, int colonne)
        {
            return ligne >= 0 && ligne < 8 && colonne >= 0 && colonne < 8;
        }

        /// <summary>
        /// Vérifie si une case est dans les coins de l'échiquier
        /// </summary>
        public static bool EstCoin(int ligne, int colonne)
        {
            return (ligne == 0 || ligne == 7) && (colonne == 0 || colonne == 7);
        }

        /// <summary>
        /// Vérifie si une case est sur le bord de l'échiquier
        /// </summary>
        public static bool EstBord(int ligne, int colonne)
        {
            return ligne == 0 || ligne == 7 || colonne == 0 || colonne == 7;
        }

        /// <summary>
        /// Vérifie si une case est dans le centre de l'échiquier (4 cases centrales)
        /// </summary>
        public static bool EstCentre(int ligne, int colonne)
        {
            return (ligne == 3 || ligne == 4) && (colonne == 3 || colonne == 4);
        }

        /// <summary>
        /// Vérifie si une case est dans le centre étendu (16 cases centrales)
        /// </summary>
        public static bool EstCentreEtendu(int ligne, int colonne)
        {
            return ligne >= 2 && ligne <= 5 && colonne >= 2 && colonne <= 5;
        }

        #endregion

        #region Couleur des cases

        /// <summary>
        /// Détermine la couleur d'une case (claire ou foncée)
        /// </summary>
        /// <param name="ligne">Ligne</param>
        /// <param name="colonne">Colonne</param>
        /// <returns>True si la case est claire</returns>
        public static bool EstCaseClaire(int ligne, int colonne)
        {
            return (ligne + colonne) % 2 == 0;
        }

        /// <summary>
        /// Obtient la couleur de case opposée
        /// </summary>
        public static bool CouleurCaseOpposee(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return EstCaseClaire(ligne1, colonne1) != EstCaseClaire(ligne2, colonne2);
        }

        #endregion

        #region Distances

        /// <summary>
        /// Calcule la distance de Manhattan entre deux cases
        /// </summary>
        public static int DistanceManhattan(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return Math.Abs(ligne2 - ligne1) + Math.Abs(colonne2 - colonne1);
        }

        /// <summary>
        /// Calcule la distance de Chebyshev (distance roi) entre deux cases
        /// </summary>
        public static int DistanceChebyshev(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return Math.Max(Math.Abs(ligne2 - ligne1), Math.Abs(colonne2 - colonne1));
        }

        /// <summary>
        /// Calcule la distance euclidienne entre deux cases
        /// </summary>
        public static double DistanceEuclidienne(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            int diffLigne = ligne2 - ligne1;
            int diffColonne = colonne2 - colonne1;
            return Math.Sqrt(diffLigne * diffLigne + diffColonne * diffColonne);
        }

        #endregion

        #region Chemins et directions

        /// <summary>
        /// Vérifie si deux cases sont sur la même ligne
        /// </summary>
        public static bool SurMemeLigne(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return ligne1 == ligne2;
        }

        /// <summary>
        /// Vérifie si deux cases sont sur la même colonne
        /// </summary>
        public static bool SurMemeColonne(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return colonne1 == colonne2;
        }

        /// <summary>
        /// Vérifie si deux cases sont sur la même diagonale
        /// </summary>
        public static bool SurMemeDiagonale(int ligne1, int colonne1, int ligne2, int colonne2)
        {
            return Math.Abs(ligne2 - ligne1) == Math.Abs(colonne2 - colonne1);
        }

        /// <summary>
        /// Obtient toutes les cases entre deux positions (exclusif)
        /// </summary>
        public static List<(int ligne, int colonne)> ObtenirCasesEntre(
            int ligne1, int colonne1, int ligne2, int colonne2)
        {
            List<(int, int)> cases = new List<(int, int)>();

            // Vérifier que c'est une ligne droite (horizontale, verticale ou diagonale)
            if (!SurMemeLigne(ligne1, colonne1, ligne2, colonne2) &&
                !SurMemeColonne(ligne1, colonne1, ligne2, colonne2) &&
                !SurMemeDiagonale(ligne1, colonne1, ligne2, colonne2))
            {
                return cases;
            }

            int dirLigne = ligne2 > ligne1 ? 1 : (ligne2 < ligne1 ? -1 : 0);
            int dirColonne = colonne2 > colonne1 ? 1 : (colonne2 < colonne1 ? -1 : 0);

            int ligneCourante = ligne1 + dirLigne;
            int colonneCourante = colonne1 + dirColonne;

            while (ligneCourante != ligne2 || colonneCourante != colonne2)
            {
                cases.Add((ligneCourante, colonneCourante));
                ligneCourante += dirLigne;
                colonneCourante += dirColonne;
            }

            return cases;
        }

        /// <summary>
        /// Obtient la direction entre deux cases
        /// </summary>
        /// <returns>Tuple (dirLigne, dirColonne) avec valeurs -1, 0 ou 1</returns>
        public static (int dirLigne, int dirColonne) ObtenirDirection(
            int ligne1, int colonne1, int ligne2, int colonne2)
        {
            int dirLigne = ligne2 > ligne1 ? 1 : (ligne2 < ligne1 ? -1 : 0);
            int dirColonne = colonne2 > colonne1 ? 1 : (colonne2 < colonne1 ? -1 : 0);
            return (dirLigne, dirColonne);
        }

        #endregion

        #region Analyse de l'échiquier

        /// <summary>
        /// Compte le nombre de pièces d'un type spécifique
        /// </summary>
        public static int CompterPieces(Echiquier echiquier, TypePiece type, CouleurPiece? couleur = null)
        {
            int compte = 0;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null && piece.Type == type)
                    {
                        if (!couleur.HasValue || piece.Couleur == couleur.Value)
                        {
                            compte++;
                        }
                    }
                }
            }

            return compte;
        }

        /// <summary>
        /// Compte le nombre total de pièces sur l'échiquier
        /// </summary>
        public static int CompterToutesPieces(Echiquier echiquier, CouleurPiece? couleur = null)
        {
            int compte = 0;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null)
                    {
                        if (!couleur.HasValue || piece.Couleur == couleur.Value)
                        {
                            compte++;
                        }
                    }
                }
            }

            return compte;
        }

        /// <summary>
        /// Calcule le score matériel d'un joueur
        /// </summary>
        public static int CalculerScoreMateriel(Echiquier echiquier, CouleurPiece couleur)
        {
            int score = 0;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null && piece.Couleur == couleur)
                    {
                        score += piece.Valeur;
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Trouve toutes les pièces d'un type et couleur spécifiques
        /// </summary>
        public static List<Piece> TrouverPieces(Echiquier echiquier, TypePiece type, CouleurPiece couleur)
        {
            List<Piece> pieces = new List<Piece>();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null && piece.Type == type && piece.Couleur == couleur)
                    {
                        pieces.Add(piece);
                    }
                }
            }

            return pieces;
        }

        /// <summary>
        /// Trouve la position d'une pièce spécifique
        /// </summary>
        public static (int ligne, int colonne)? TrouverPosition(Echiquier echiquier, Piece pieceRecherchee)
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece == pieceRecherchee)
                    {
                        return (ligne, colonne);
                    }
                }
            }

            return null;
        }

        #endregion

        #region Cases spéciales

        /// <summary>
        /// Obtient toutes les cases vides de l'échiquier
        /// </summary>
        public static List<(int ligne, int colonne)> ObtenirCasesVides(Echiquier echiquier)
        {
            List<(int, int)> cases = new List<(int, int)>();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    if (echiquier.ObtenirPiece(ligne, colonne) == null)
                    {
                        cases.Add((ligne, colonne));
                    }
                }
            }

            return cases;
        }

        /// <summary>
        /// Obtient toutes les cases occupées
        /// </summary>
        public static List<(int ligne, int colonne)> ObtenirCasesOccupees(Echiquier echiquier, CouleurPiece? couleur = null)
        {
            List<(int, int)> cases = new List<(int, int)>();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null && (!couleur.HasValue || piece.Couleur == couleur.Value))
                    {
                        cases.Add((ligne, colonne));
                    }
                }
            }

            return cases;
        }

        /// <summary>
        /// Obtient toutes les cases adjacentes à une position
        /// </summary>
        public static List<(int ligne, int colonne)> ObtenirCasesAdjacentes(int ligne, int colonne, bool incluantDiagonales = true)
        {
            List<(int, int)> cases = new List<(int, int)>();

            // Directions cardinales
            int[,] directions = incluantDiagonales
                ? new int[,] { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } }
                : new int[,] { { -1, 0 }, { 0, -1 }, { 0, 1 }, { 1, 0 } };

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int nouvelleLigne = ligne + directions[i, 0];
                int nouvelleColonne = colonne + directions[i, 1];

                if (EstPositionValide(nouvelleLigne, nouvelleColonne))
                {
                    cases.Add((nouvelleLigne, nouvelleColonne));
                }
            }

            return cases;
        }

        #endregion

        #region Symétrie et rotation

        /// <summary>
        /// Obtient la position symétrique horizontale (miroir)
        /// </summary>
        public static (int ligne, int colonne) SymetrieHorizontale(int ligne, int colonne)
        {
            return (ligne, 7 - colonne);
        }

        /// <summary>
        /// Obtient la position symétrique verticale
        /// </summary>
        public static (int ligne, int colonne) SymetrieVerticale(int ligne, int colonne)
        {
            return (7 - ligne, colonne);
        }

        /// <summary>
        /// Obtient la position après rotation de 90° dans le sens horaire
        /// </summary>
        public static (int ligne, int colonne) Rotation90Horaire(int ligne, int colonne)
        {
            return (colonne, 7 - ligne);
        }

        /// <summary>
        /// Obtient la position après rotation de 90° dans le sens antihoraire
        /// </summary>
        public static (int ligne, int colonne) Rotation90Antihoraire(int ligne, int colonne)
        {
            return (7 - colonne, ligne);
        }

        /// <summary>
        /// Obtient la position après rotation de 180°
        /// </summary>
        public static (int ligne, int colonne) Rotation180(int ligne, int colonne)
        {
            return (7 - ligne, 7 - colonne);
        }

        #endregion

        #region Zones de l'échiquier

        /// <summary>
        /// Détermine dans quelle moitié de l'échiquier se trouve une case
        /// </summary>
        public static string ObtenirMoitie(int ligne, CouleurPiece perspective)
        {
            if (perspective == CouleurPiece.Blanc)
            {
                return ligne >= 4 ? "Moitié adverse" : "Moitié alliée";
            }
            else
            {
                return ligne < 4 ? "Moitié adverse" : "Moitié alliée";
            }
        }

        /// <summary>
        /// Obtient le camp d'une case (1-3 premières rangées pour chaque joueur)
        /// </summary>
        public static string ObtenirCamp(int ligne)
        {
            if (ligne <= 2)
                return "Camp noir";
            else if (ligne >= 5)
                return "Camp blanc";
            else
                return "Zone neutre";
        }

        /// <summary>
        /// Vérifie si une case est sur la ligne de promotion pour une couleur
        /// </summary>
        public static bool EstLignePromotion(int ligne, CouleurPiece couleur)
        {
            return (couleur == CouleurPiece.Blanc && ligne == 0) ||
                   (couleur == CouleurPiece.Noir && ligne == 7);
        }

        /// <summary>
        /// Vérifie si une case est sur la ligne de départ des pions
        /// </summary>
        public static bool EstLigneDepartPions(int ligne, CouleurPiece couleur)
        {
            return (couleur == CouleurPiece.Blanc && ligne == 6) ||
                   (couleur == CouleurPiece.Noir && ligne == 1);
        }

        #endregion

        #region Conversion de noms

        /// <summary>
        /// Obtient le nom complet d'une colonne
        /// </summary>
        public static string ObtenirNomColonne(int colonne)
        {
            return colonne switch
            {
                0 => "Colonne A",
                1 => "Colonne B",
                2 => "Colonne C",
                3 => "Colonne D",
                4 => "Colonne E",
                5 => "Colonne F",
                6 => "Colonne G",
                7 => "Colonne H",
                _ => "Invalide"
            };
        }

        /// <summary>
        /// Obtient le nom complet d'une ligne
        /// </summary>
        public static string ObtenirNomLigne(int ligne)
        {
            return $"Ligne {8 - ligne}";
        }

        /// <summary>
        /// Obtient le nom complet d'une case
        /// </summary>
        public static string ObtenirNomCase(int ligne, int colonne)
        {
            return UtilitaireNotation.VersNotationAlgebrique(ligne, colonne);
        }

        #endregion

        #region Hash et comparaison

        /// <summary>
        /// Génère un hash simple pour une position d'échiquier (Zobrist simplifié)
        /// </summary>
        public static long GenererHashPosition(Echiquier echiquier)
        {
            long hash = 0;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece = echiquier.ObtenirPiece(ligne, colonne);
                    if (piece != null)
                    {
                        int pieceValue = ((int)piece.Type + 1) * ((int)piece.Couleur + 1);
                        hash ^= (long)pieceValue * (ligne * 8 + colonne + 1);
                    }
                }
            }

            return hash;
        }

        /// <summary>
        /// Compare deux positions d'échiquier
        /// </summary>
        public static bool PositionsIdentiques(Echiquier echiquier1, Echiquier echiquier2)
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece piece1 = echiquier1.ObtenirPiece(ligne, colonne);
                    Piece piece2 = echiquier2.ObtenirPiece(ligne, colonne);

                    if ((piece1 == null) != (piece2 == null))
                        return false;

                    if (piece1 != null && (piece1.Type != piece2.Type || piece1.Couleur != piece2.Couleur))
                        return false;
                }
            }

            return true;
        }

        #endregion
    }
}