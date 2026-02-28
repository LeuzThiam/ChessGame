using ChessGame.Core.Domain.Models.Pieces;
using System;

namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Représente un coup joué aux échecs
    /// </summary>
    public class Coup
    {
        #region Propriétés de base

        /// <summary>
        /// Pièce qui effectue le coup
        /// </summary>
        public Piece? Piece { get; set; }

        /// <summary>
        /// Ligne de départ
        /// </summary>
        public int LigneDepart { get; set; }

        /// <summary>
        /// Colonne de départ
        /// </summary>
        public int ColonneDepart { get; set; }

        /// <summary>
        /// Ligne d'arrivée
        /// </summary>
        public int LigneArrivee { get; set; }

        /// <summary>
        /// Colonne d'arrivée
        /// </summary>
        public int ColonneArrivee { get; set; }

        /// <summary>
        /// Pièce capturée (null si pas de capture)
        /// </summary>
        public Piece? PieceCapturee { get; set; }

        #endregion

        #region Propriétés de type de coup

        /// <summary>
        /// Indique si c'est un petit roque
        /// </summary>
        public bool EstPetitRoque { get; set; }

        /// <summary>
        /// Indique si c'est un grand roque
        /// </summary>
        public bool EstGrandRoque { get; set; }

        /// <summary>
        /// Indique si c'est une prise en passant
        /// </summary>
        public bool EstEnPassant { get; set; }

        /// <summary>
        /// Indique si c'est une promotion de pion
        /// </summary>
        public bool EstPromotion { get; set; }

        /// <summary>
        /// Type de pièce en cas de promotion (Reine par défaut)
        /// </summary>
        public TypePiece PiecePromotion { get; set; }

        /// <summary>
        /// Indique si le coup met le roi adverse en échec
        /// </summary>
        public bool DonneEchec { get; set; }

        /// <summary>
        /// Indique si le coup met le roi adverse en échec et mat
        /// </summary>
        public bool DonneEchecEtMat { get; set; }

        #endregion

        #region Propriétés calculées

        /// <summary>
        /// Type du coup
        /// </summary>
        public TypeCoup TypeCoup
        {
            get
            {
                if (EstPetitRoque) return TypeCoup.PetitRoque;
                if (EstGrandRoque) return TypeCoup.GrandRoque;
                if (EstEnPassant) return TypeCoup.EnPassant;
                if (EstPromotion) return TypeCoup.Promotion;
                if (PieceCapturee != null) return TypeCoup.Capture;
                return TypeCoup.Normal;
            }
        }

        /// <summary>
        /// Notation algébrique du coup (ex: "e4", "Nf3", "O-O")
        /// </summary>
        public string NotationAlgebrique
        {
            get
            {
                if (EstPetitRoque) return "O-O";
                if (EstGrandRoque) return "O-O-O";

                string notation = "";

                // Ajouter la lettre de la pièce (sauf pour le pion)
                if (Piece != null && Piece.Type != TypePiece.Pion)
                {
                    notation += ObtenirSymbolePiece(Piece.Type);
                }

                // Position de départ (pour lever l'ambiguïté si nécessaire)
                // Cette partie devrait être améliorée avec la logique complète

                // Capture
                if (PieceCapturee != null)
                {
                    if (Piece?.Type == TypePiece.Pion)
                    {
                        notation += (char)('a' + ColonneDepart);
                    }
                    notation += "x";
                }

                // Position d'arrivée
                notation += $"{(char)('a' + ColonneArrivee)}{8 - LigneArrivee}";

                // Promotion
                if (EstPromotion)
                {
                    notation += $"={ObtenirSymbolePiece(PiecePromotion)}";
                }

                // Échec ou échec et mat
                if (DonneEchecEtMat)
                    notation += "#";
                else if (DonneEchec)
                    notation += "+";

                return notation;
            }
        }

        /// <summary>
        /// Notation longue (ex: "e2-e4")
        /// </summary>
        public string NotationLongue
        {
            get
            {
                string depart = $"{(char)('a' + ColonneDepart)}{8 - LigneDepart}";
                string arrivee = $"{(char)('a' + ColonneArrivee)}{8 - LigneArrivee}";
                return $"{depart}-{arrivee}";
            }
        }

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Coup()
        {
            Piece = null;
            PieceCapturee = null;
            PiecePromotion = TypePiece.Reine; // Par défaut, on promeut en reine
        }

        /// <summary>
        /// Constructeur complet
        /// </summary>
        public Coup(Piece? piece, int ligneDepart, int colonneDepart,
                    int ligneArrivee, int colonneArrivee, Piece? pieceCapturee = null)
        {
            Piece = piece;
            LigneDepart = ligneDepart;
            ColonneDepart = colonneDepart;
            LigneArrivee = ligneArrivee;
            ColonneArrivee = colonneArrivee;
            PieceCapturee = pieceCapturee;
            PiecePromotion = TypePiece.Reine;
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Obtient le symbole d'une pièce pour la notation algébrique
        /// </summary>
        private string ObtenirSymbolePiece(TypePiece type)
        {
            return type switch
            {
                TypePiece.Roi => "K",
                TypePiece.Reine => "Q",
                TypePiece.Tour => "R",
                TypePiece.Fou => "B",
                TypePiece.Cavalier => "N",
                TypePiece.Pion => "",
                _ => ""
            };
        }

        /// <summary>
        /// Vérifie si le coup est une capture
        /// </summary>
        public bool EstCapture()
        {
            return PieceCapturee != null || EstEnPassant;
        }

        /// <summary>
        /// Clone le coup
        /// </summary>
        public Coup Cloner()
        {
            return new Coup(Piece?.Cloner(), LigneDepart, ColonneDepart, LigneArrivee, ColonneArrivee, PieceCapturee?.Cloner())
            {
                EstPetitRoque = this.EstPetitRoque,
                EstGrandRoque = this.EstGrandRoque,
                EstEnPassant = this.EstEnPassant,
                EstPromotion = this.EstPromotion,
                PiecePromotion = this.PiecePromotion,
                DonneEchec = this.DonneEchec,
                DonneEchecEtMat = this.DonneEchecEtMat
            };
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return NotationAlgebrique;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Coup autreCoup)
            {
                return LigneDepart == autreCoup.LigneDepart &&
                       ColonneDepart == autreCoup.ColonneDepart &&
                       LigneArrivee == autreCoup.LigneArrivee &&
                       ColonneArrivee == autreCoup.ColonneArrivee &&
                       EstPromotion == autreCoup.EstPromotion &&
                       (!EstPromotion || PiecePromotion == autreCoup.PiecePromotion);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LigneDepart, ColonneDepart, LigneArrivee, ColonneArrivee);
        }

        #endregion
    }
}
