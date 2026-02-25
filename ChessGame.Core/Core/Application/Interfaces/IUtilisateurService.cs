using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessGame.Core.Application.Models;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Service pour gérer les utilisateurs (connexion optionnelle).
    /// </summary>
    public interface IUtilisateurService
    {
        Task<IReadOnlyList<UtilisateurInfo>> ObtenirTousAsync();
        Task<UtilisateurInfo> CreerOuRecupererAsync(string nom, string? email = null);
        Task BasculerUtilisateurAsync(Guid id);
        void BasculerInvite(string? nom = "Invité");
        IUtilisateurSession Session { get; }
    }
}
