using ChessGame.Core.Application.Interfaces;
using System;

namespace ChessGameMAUI.Services
{
    /// <summary>
    /// Implémentation simple de session utilisateur (profil optionnel).
    /// </summary>
    public class UtilisateurSession : IUtilisateurSession
    {
        public Guid? CurrentUserId { get; private set; }
        public string CurrentUserName { get; private set; } = "Invité";
        public bool IsLoggedIn { get; private set; }
        public bool IsGuest => !IsLoggedIn;

        public void SetUser(Guid id, string nom)
        {
            CurrentUserId = id;
            CurrentUserName = string.IsNullOrWhiteSpace(nom) ? "Utilisateur" : nom;
            IsLoggedIn = true;
        }

        public void SetGuest(string nom = "Invité")
        {
            CurrentUserId = null;
            CurrentUserName = string.IsNullOrWhiteSpace(nom) ? "Invité" : nom;
            IsLoggedIn = false;
        }

        public void Clear()
        {
            SetGuest();
        }
    }
}
