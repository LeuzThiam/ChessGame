using ChessGame.Core.Domain.Models;

namespace ChessGame.Core.Application.Interfaces
{
    public interface IPartieRepository
    {
        void EnregistrerPartie(EtatPartie etatPartie, string pgn, string? fen);
        IQueryable<object> ObtenirToutes();
        object? ObtenirParId(Guid id);
        void Supprimer(Guid id);
    }
}
