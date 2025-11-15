using ChessGame.Models;
using ChessGame.Models.Pieces;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ChessGame.Utilitaires
{
    /// <summary>
    /// Utilitaire pour la conversion entre différentes notations d'échecs
    /// </summary>
    public static class UtilitaireNotation
    {
        #region Conversion Coordonnées <-> Notation Algébrique

        /// <summary>
        /// Convertit des coordonnées en notation algébrique (ex: 6,4 -> "e2")
        /// </summary>
        /// <param name="ligne">Ligne (0-7)</param>
        /// <param name="colonne">Colonne (0-7)</param>
        /// <returns>Notation algébrique (ex: "e4")</returns>
        public static string VersNotationAlgebrique(int ligne, int colonne)
        {
            if (ligne < 0 || ligne > 7 || colonne < 0 || colonne > 7)
                throw new ArgumentException("Coordonnées invalides");

            char colonneChar = (char)('a' + colonne);
            int ligneNum = 8 - ligne;
            return $"{colonneChar}{ligneNum}";
        }

        /// <summary>
        /// Convertit une notation algébrique en coordonnées (ex: "e4" -> (4,4))
        /// </summary>
        /// <param name="notation">Notation algébrique (ex: "e4")</param>
        /// <returns>Tuple (ligne, colonne)</returns>
        public static (int ligne, int colonne) DepuisNotationAlgebrique(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length < 2)
                throw new ArgumentException("Notation invalide");

            notation = notation.ToLower().Trim();

            char colonneChar = notation[0];
            char ligneChar = notation[1];

            if (colonneChar < 'a' || colonneChar > 'h')
                throw new ArgumentException("Colonne invalide");

            if (ligneChar < '1' || ligneChar > '8')
                throw new ArgumentException("Ligne invalide");

            int colonne = colonneChar - 'a';
            int ligne = 8 - (ligneChar - '0');

            return (ligne, colonne);
        }

        #endregion

        #region Notation PGN

        /// <summary>
        /// Convertit un coup en notation PGN standard
        /// </summary>
        /// <param name="coup">Le coup à convertir</param>
        /// <returns>Notation PGN (ex: "Nf3", "e4", "O-O")</returns>
        public static string VersPGN(Coup coup)
        {
            if (coup == null)
                return "";

            StringBuilder notation = new StringBuilder();

            // Roques
            if (coup.EstPetitRoque)
                return "O-O";
            if (coup.EstGrandRoque)
                return "O-O-O";

            // Symbole de la pièce (sauf pion)
            if (coup.Piece.Type != TypePiece.Pion)
            {
                notation.Append(ObtenirSymbolePiece(coup.Piece.Type));
            }

            // Pour les pions qui capturent, ajouter la colonne de départ
            if (coup.Piece.Type == TypePiece.Pion && coup.EstCapture())
            {
                notation.Append((char)('a' + coup.ColonneDepart));
            }

            // Symbole de capture
            if (coup.EstCapture())
            {
                notation.Append('x');
            }

            // Case d'arrivée
            notation.Append(VersNotationAlgebrique(coup.LigneArrivee, coup.ColonneArrivee));

            // Promotion
            if (coup.EstPromotion)
            {
                notation.Append('=');
                notation.Append(ObtenirSymbolePiece(coup.PiecePromotion));
            }

            // En passant
            if (coup.EstEnPassant)
            {
                notation.Append(" e.p.");
            }

            // Échec et mat
            if (coup.DonneEchecEtMat)
                notation.Append('#');
            else if (coup.DonneEchec)
                notation.Append('+');

            return notation.ToString();
        }

        /// <summary>
        /// Parse une notation PGN simple en coordonnées
        /// </summary>
        /// <param name="notation">Notation PGN (ex: "e4", "Nf3")</param>
        /// <returns>Informations sur le coup</returns>
        public static InfoCoupPGN ParserPGN(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation))
                return null;

            notation = notation.Trim();
            InfoCoupPGN info = new InfoCoupPGN();

            // Nettoyer les annotations
            notation = notation.Replace("+", "").Replace("#", "").Replace("!", "")
                              .Replace("?", "").Replace(" e.p.", "").Trim();

            // Roques
            if (notation == "O-O" || notation == "0-0")
            {
                info.EstPetitRoque = true;
                return info;
            }
            if (notation == "O-O-O" || notation == "0-0-0")
            {
                info.EstGrandRoque = true;
                return info;
            }

            // Déterminer le type de pièce
            if (char.IsUpper(notation[0]))
            {
                info.TypePiece = ObtenirTypePieceDepuisSymbole(notation[0]);
                notation = notation.Substring(1);
            }
            else
            {
                info.TypePiece = TypePiece.Pion;
            }

            // Capture
            if (notation.Contains("x"))
            {
                info.EstCapture = true;
                notation = notation.Replace("x", "");
            }

            // Promotion
            if (notation.Contains("="))
            {
                int indexPromotion = notation.IndexOf('=');
                if (indexPromotion + 1 < notation.Length)
                {
                    info.PiecePromotion = ObtenirTypePieceDepuisSymbole(notation[indexPromotion + 1]);
                }
                notation = notation.Substring(0, indexPromotion);
            }

            // Extraire la case de destination (derniers 2 caractères)
            if (notation.Length >= 2)
            {
                string caseDestination = notation.Substring(notation.Length - 2);
                var coords = DepuisNotationAlgebrique(caseDestination);
                info.LigneArrivee = coords.ligne;
                info.ColonneArrivee = coords.colonne;
            }

            // Informations de désambiguïsation (avant la case de destination)
            if (notation.Length > 2)
            {
                string disamb = notation.Substring(0, notation.Length - 2);

                // Colonne de départ
                if (disamb.Length > 0 && disamb[0] >= 'a' && disamb[0] <= 'h')
                {
                    info.ColonneDepart = disamb[0] - 'a';
                }

                // Ligne de départ
                if (disamb.Length > 1 && char.IsDigit(disamb[1]))
                {
                    info.LigneDepart = 8 - (disamb[1] - '0');
                }
            }

            return info;
        }

        #endregion

        #region Notation Longue

        /// <summary>
        /// Convertit un coup en notation longue (ex: "e2e4")
        /// </summary>
        /// <param name="coup">Le coup à convertir</param>
        /// <returns>Notation longue</returns>
        public static string VersNotationLongue(Coup coup)
        {
            if (coup == null)
                return "";

            string depart = VersNotationAlgebrique(coup.LigneDepart, coup.ColonneDepart);
            string arrivee = VersNotationAlgebrique(coup.LigneArrivee, coup.ColonneArrivee);

            StringBuilder notation = new StringBuilder();
            notation.Append(depart);
            notation.Append(arrivee);

            // Promotion
            if (coup.EstPromotion)
            {
                notation.Append(ObtenirSymbolePiece(coup.PiecePromotion).ToLower());
            }

            return notation.ToString();
        }

        /// <summary>
        /// Parse une notation longue (ex: "e2e4", "e7e8q")
        /// </summary>
        /// <param name="notation">Notation longue</param>
        /// <returns>Informations sur le coup</returns>
        public static InfoCoupLongue ParserNotationLongue(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation) || notation.Length < 4)
                return null;

            notation = notation.Trim().ToLower();

            InfoCoupLongue info = new InfoCoupLongue();

            // Cases de départ et d'arrivée
            string caseDepart = notation.Substring(0, 2);
            string caseArrivee = notation.Substring(2, 2);

            var coordsDepart = DepuisNotationAlgebrique(caseDepart);
            var coordsArrivee = DepuisNotationAlgebrique(caseArrivee);

            info.LigneDepart = coordsDepart.ligne;
            info.ColonneDepart = coordsDepart.colonne;
            info.LigneArrivee = coordsArrivee.ligne;
            info.ColonneArrivee = coordsArrivee.colonne;

            // Promotion (5ème caractère optionnel)
            if (notation.Length > 4)
            {
                char symbolePromotion = char.ToUpper(notation[4]);
                info.PiecePromotion = ObtenirTypePieceDepuisSymbole(symbolePromotion);
            }

            return info;
        }

        #endregion

        #region UCI (Universal Chess Interface)

        /// <summary>
        /// Convertit un coup en notation UCI (ex: "e2e4")
        /// </summary>
        /// <param name="coup">Le coup à convertir</param>
        /// <returns>Notation UCI</returns>
        public static string VersUCI(Coup coup)
        {
            // UCI est identique à la notation longue
            return VersNotationLongue(coup);
        }

        /// <summary>
        /// Parse une notation UCI
        /// </summary>
        /// <param name="notation">Notation UCI</param>
        /// <returns>Informations sur le coup</returns>
        public static InfoCoupLongue ParserUCI(string notation)
        {
            return ParserNotationLongue(notation);
        }

        #endregion

        #region Symboles de pièces

        /// <summary>
        /// Obtient le symbole d'une pièce pour la notation (anglais)
        /// </summary>
        /// <param name="type">Type de la pièce</param>
        /// <returns>Symbole (K, Q, R, B, N, P)</returns>
        public static string ObtenirSymbolePiece(TypePiece type)
        {
            return type switch
            {
                TypePiece.Roi => "K",
                TypePiece.Reine => "Q",
                TypePiece.Tour => "R",
                TypePiece.Fou => "B",
                TypePiece.Cavalier => "N",
                TypePiece.Pion => "P",
                _ => ""
            };
        }

        /// <summary>
        /// Obtient le symbole d'une pièce en français
        /// </summary>
        /// <param name="type">Type de la pièce</param>
        /// <returns>Symbole (R, D, T, F, C, P)</returns>
        public static string ObtenirSymbolePieceFrancais(TypePiece type)
        {
            return type switch
            {
                TypePiece.Roi => "R",
                TypePiece.Reine => "D",
                TypePiece.Tour => "T",
                TypePiece.Fou => "F",
                TypePiece.Cavalier => "C",
                TypePiece.Pion => "P",
                _ => ""
            };
        }

        /// <summary>
        /// Obtient le type de pièce depuis un symbole
        /// </summary>
        /// <param name="symbole">Symbole (K, Q, R, B, N, P)</param>
        /// <returns>Type de pièce</returns>
        public static TypePiece ObtenirTypePieceDepuisSymbole(char symbole)
        {
            return char.ToUpper(symbole) switch
            {
                'K' => TypePiece.Roi,
                'Q' => TypePiece.Reine,
                'R' => TypePiece.Tour,
                'B' => TypePiece.Fou,
                'N' => TypePiece.Cavalier,
                'P' => TypePiece.Pion,
                _ => TypePiece.Pion
            };
        }

        /// <summary>
        /// Obtient le symbole Unicode d'une pièce
        /// </summary>
        /// <param name="type">Type de pièce</param>
        /// <param name="couleur">Couleur de la pièce</param>
        /// <returns>Caractère Unicode</returns>
        public static string ObtenirSymboleUnicode(TypePiece type, CouleurPiece couleur)
        {
            if (couleur == CouleurPiece.Blanc)
            {
                return type switch
                {
                    TypePiece.Roi => "♔",
                    TypePiece.Reine => "♕",
                    TypePiece.Tour => "♖",
                    TypePiece.Fou => "♗",
                    TypePiece.Cavalier => "♘",
                    TypePiece.Pion => "♙",
                    _ => ""
                };
            }
            else
            {
                return type switch
                {
                    TypePiece.Roi => "♚",
                    TypePiece.Reine => "♛",
                    TypePiece.Tour => "♜",
                    TypePiece.Fou => "♝",
                    TypePiece.Cavalier => "♞",
                    TypePiece.Pion => "♟",
                    _ => ""
                };
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Vérifie si une notation algébrique est valide
        /// </summary>
        /// <param name="notation">Notation à vérifier</param>
        /// <returns>True si valide</returns>
        public static bool EstNotationAlgebriqueValide(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation))
                return false;

            notation = notation.Trim().ToLower();

            // Format: [a-h][1-8]
            return Regex.IsMatch(notation, @"^[a-h][1-8]$");
        }

        /// <summary>
        /// Vérifie si une notation PGN est valide (format basique)
        /// </summary>
        /// <param name="notation">Notation à vérifier</param>
        /// <returns>True si valide</returns>
        public static bool EstNotationPGNValide(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation))
                return false;

            notation = notation.Trim();

            // Roques
            if (notation == "O-O" || notation == "O-O-O" ||
                notation == "0-0" || notation == "0-0-0")
                return true;

            // Nettoyer les annotations
            notation = notation.Replace("+", "").Replace("#", "")
                              .Replace("!", "").Replace("?", "")
                              .Replace(" e.p.", "").Trim();

            // Format basique: [KQRBN]?[a-h]?[1-8]?x?[a-h][1-8](=[QRBN])?
            return Regex.IsMatch(notation,
                @"^[KQRBN]?[a-h]?[1-8]?x?[a-h][1-8](=[QRBN])?$",
                RegexOptions.IgnoreCase);
        }

        #endregion
    }

    #region Classes d'information

    /// <summary>
    /// Informations extraites d'une notation PGN
    /// </summary>
    public class InfoCoupPGN
    {
        public TypePiece TypePiece { get; set; }
        public int? LigneDepart { get; set; }
        public int? ColonneDepart { get; set; }
        public int LigneArrivee { get; set; }
        public int ColonneArrivee { get; set; }
        public bool EstCapture { get; set; }
        public bool EstPetitRoque { get; set; }
        public bool EstGrandRoque { get; set; }
        public TypePiece? PiecePromotion { get; set; }
    }

    /// <summary>
    /// Informations extraites d'une notation longue
    /// </summary>
    public class InfoCoupLongue
    {
        public int LigneDepart { get; set; }
        public int ColonneDepart { get; set; }
        public int LigneArrivee { get; set; }
        public int ColonneArrivee { get; set; }
        public TypePiece? PiecePromotion { get; set; }
    }

    #endregion
}