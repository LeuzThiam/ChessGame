using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Application.Services;

namespace ChessGameMAUI.Presentation.Services
{
    /// <summary>
    /// Wrapper pour SimpleAi du Core - pour compatibilité avec le projet MAUI.
    /// L'implémentation réelle est maintenant dans ChessGame.Core.Application.Services.SimpleAi
    /// </summary>
    public static class SimpleAi
    {
        // Utilise le constructeur par défaut qui fonctionne en mode dégradé (sans Minimax)
        // Pour bénéficier de Minimax, il faudrait injecter les dépendances via DI
        private static readonly ISimpleAi _simpleAi = new ChessGame.Core.Application.Services.SimpleAi();

        public static void Attacher(IServicePartie service, CouleurPiece couleurIa, int niveau = 1)
        {
            _simpleAi.Attacher(service, couleurIa, niveau);
        }
    }
}
