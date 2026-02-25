using ChessGame.Core.Domain.Models;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour le moteur d'IA utilisant Minimax avec élagage Alpha-Beta
    /// </summary>
    public interface IMoteurMinimax
    {
        /// <summary>
        /// Trouve le meilleur coup pour une couleur donnée
        /// </summary>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <param name="couleur">La couleur qui doit jouer</param>
        /// <param name="profondeur">La profondeur de recherche (nombre de demi-coups)</param>
        /// <returns>Le meilleur coup trouvé, ou null si aucun coup n'est disponible</returns>
        Coup? MeilleurCoup(Echiquier echiquier, EtatPartie etatPartie, CouleurPiece couleur, int profondeur);

        /// <summary>
        /// Évalue une position avec Minimax et Alpha-Beta
        /// </summary>
        /// <param name="echiquier">L'échiquier à évaluer</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <param name="profondeur">Profondeur de recherche</param>
        /// <param name="alpha">Valeur Alpha pour l'élagage</param>
        /// <param name="beta">Valeur Beta pour l'élagage</param>
        /// <param name="maximisant">True si on maximise (pour la couleur à jouer), false sinon</param>
        /// <param name="couleur">La couleur qui doit jouer</param>
        /// <returns>Score de la position</returns>
        int Minimax(Echiquier echiquier, EtatPartie etatPartie, int profondeur, int alpha, int beta, bool maximisant, CouleurPiece couleur);
    }
}

