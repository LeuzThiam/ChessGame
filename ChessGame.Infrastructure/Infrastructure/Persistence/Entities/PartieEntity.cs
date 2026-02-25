using ChessGame.Core.Domain.Models;
using System;
using System.Collections.Generic;

namespace ChessGame.Infrastructure.Persistence.Entities
{
    public class PartieEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public StatutPartie Statut { get; set; }
        public TypeFinPartie TypeFin { get; set; }

        public string JoueurBlancNom { get; set; } = string.Empty;
        public string JoueurNoirNom { get; set; } = string.Empty;
        public Guid? JoueurBlancId { get; set; }
        public Guid? JoueurNoirId { get; set; }
        public string? GagnantNom { get; set; }

        public TimeSpan TempsBlancRestant { get; set; }
        public TimeSpan TempsNoirRestant { get; set; }

        public string? PGN { get; set; }
        public string? FEN { get; set; }

        public int NombreCoups { get; set; }

        public ICollection<CoupEntity> Coups { get; set; } = new List<CoupEntity>();
        public UtilisateurEntity? JoueurBlanc { get; set; }
        public UtilisateurEntity? JoueurNoir { get; set; }
    }
}

