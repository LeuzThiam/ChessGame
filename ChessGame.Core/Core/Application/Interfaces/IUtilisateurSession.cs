using System;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Session utilisateur optionnelle (profil connecté ou invité).
    /// </summary>
    public interface IUtilisateurSession
    {
        Guid? CurrentUserId { get; }
        string CurrentUserName { get; }
        bool IsLoggedIn { get; }
        bool IsGuest { get; }

        void SetUser(Guid id, string nom);
        void SetGuest(string nom = "Invité");
        void Clear();
    }
}
