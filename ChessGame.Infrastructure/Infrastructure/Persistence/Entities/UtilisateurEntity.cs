using System;
using System.Collections.Generic;

namespace ChessGame.Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Utilisateur inscrit (pour associer des parties à des joueurs persistants).
    /// </summary>
    public class UtilisateurEntity
    {
        public Guid Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string? Email { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public ICollection<PartieEntity> PartiesEnBlanc { get; set; } = new List<PartieEntity>();

        public ICollection<PartieEntity> PartiesEnNoir { get; set; } = new List<PartieEntity>();
    }
}
