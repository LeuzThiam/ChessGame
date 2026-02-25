using ChessGame.Core.Domain.Models;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour les règles du jeu d'échecs
    /// </summary>
    public interface IReglesJeu
    {
        /// <summary>
        /// Vérifie si un joueur est en échec
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le joueur est en échec</returns>
        bool EstEnEchec(CouleurPiece couleur, Echiquier echiquier);

        /// <summary>
        /// Vérifie si un joueur est en échec et mat
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le joueur est en échec et mat</returns>
        bool EstEchecEtMat(CouleurPiece couleur, Echiquier echiquier);

        /// <summary>
        /// Vérifie si la partie est en situation de pat
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si c'est un pat</returns>
        bool EstPat(CouleurPiece couleur, Echiquier echiquier);

        /// <summary>
        /// Vérifie si la partie est nulle par matériel insuffisant
        /// </summary>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le matériel est insuffisant pour mater</returns>
        bool EstMaterielInsuffisant(Echiquier echiquier);

        /// <summary>
        /// Vérifie la règle des 50 coups
        /// </summary>
        /// <param name="etatPartie">État de la partie</param>
        /// <returns>True si la règle des 50 coups est atteinte</returns>
        bool EstRegleDes50Coups(EtatPartie etatPartie);

        /// <summary>
        /// Vérifie la répétition triple de position
        /// </summary>
        /// <param name="etatPartie">État de la partie</param>
        /// <param name="positionFEN">Position actuelle en notation FEN</param>
        /// <returns>True si répétition triple détectée</returns>
        bool EstRepetitionTriple(EtatPartie etatPartie, string positionFEN);

        /// <summary>
        /// Vérifie si une case est attaquée
        /// </summary>
        /// <param name="ligne">Ligne de la case</param>
        /// <param name="colonne">Colonne de la case</param>
        /// <param name="couleurDefenseur">Couleur du défenseur</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si la case est attaquée</returns>
        bool EstCaseAttaquee(int ligne, int colonne, CouleurPiece couleurDefenseur, Echiquier echiquier);

        /// <summary>
        /// Détermine le statut de la partie après un coup
        /// </summary>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <param name="etatPartie">État de la partie</param>
        /// <returns>Le nouveau statut de la partie</returns>
        StatutPartie DeterminerStatutPartie(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Vérifie si la partie est terminée
        /// </summary>
        /// <param name="statut">Statut actuel de la partie</param>
        /// <returns>True si la partie est terminée</returns>
        bool EstPartieTerminee(StatutPartie statut);
    }
}