using ChessGame.Core.Domain.Models;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour l'intelligence artificielle simple du jeu d'échecs.
    /// </summary>
    public interface ISimpleAi
    {
        /// <summary>
        /// Attache l'IA à une partie pour jouer automatiquement.
        /// </summary>
        /// <param name="service">Le service de partie</param>
        /// <param name="couleurIa">La couleur que l'IA doit jouer</param>
        /// <param name="niveau">Le niveau de l'IA (1-6)</param>
        void Attacher(IServicePartie service, CouleurPiece couleurIa, int niveau = 1);

        /// <summary>
        /// Choisit le meilleur coup selon le niveau de l'IA.
        /// </summary>
        /// <param name="coups">Liste des coups possibles</param>
        /// <param name="niveau">Niveau de l'IA</param>
        /// <returns>Le coup choisi</returns>
        Coup ChoisirMeilleurCoup(List<Coup> coups, int niveau = 1);
    }
}

