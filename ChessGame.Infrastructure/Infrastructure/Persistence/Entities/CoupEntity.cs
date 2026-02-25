using ChessGame.Core.Domain.Models;
using System;

namespace ChessGame.Infrastructure.Persistence.Entities
{
    public class CoupEntity
    {
        public int Id { get; set; }

        public Guid PartieId { get; set; }
        public PartieEntity? Partie { get; set; }

        public int Ordre { get; set; }

        public int LigneDepart { get; set; }
        public int ColonneDepart { get; set; }
        public int LigneArrivee { get; set; }
        public int ColonneArrivee { get; set; }

        public TypePiece PieceType { get; set; }
        public CouleurPiece PieceCouleur { get; set; }
        public TypePiece PiecePromotion { get; set; }

        public bool EstCapture { get; set; }
        public bool EstPromotion { get; set; }
        public bool EstEnPassant { get; set; }
        public bool EstPetitRoque { get; set; }
        public bool EstGrandRoque { get; set; }
        public bool DonneEchec { get; set; }
        public bool DonneEchecEtMat { get; set; }

        public string Notation { get; set; } = string.Empty;
    }
}

