using System;

namespace ChessGame.Core.Application.Models
{
    /// <summary>
    /// DTO simple pour représenter un utilisateur.
    /// </summary>
    public class UtilisateurInfo
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
