using ChessGame.Core.Domain.Models;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour l'évaluation de position aux échecs
    /// </summary>
    public interface IEvaluationPosition
    {
        /// <summary>
        /// Évalue une position complète depuis le point de vue d'une couleur
        /// </summary>
        /// <param name="echiquier">L'échiquier à évaluer</param>
        /// <param name="couleur">La couleur pour laquelle on évalue (positif = avantage pour cette couleur)</param>
        /// <returns>Score de la position (positif = avantage pour la couleur, négatif = avantage pour l'adversaire)</returns>
        int Evaluer(Echiquier echiquier, CouleurPiece couleur);

        /// <summary>
        /// Évalue un coup spécifique dans une position
        /// </summary>
        /// <param name="coup">Le coup à évaluer</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>Score du coup</returns>
        int EvaluerCoup(Coup coup, Echiquier echiquier);
    }
}

