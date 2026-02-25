using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using System.Collections.Generic;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour la validation des coups d'échecs
    /// </summary>
    public interface IValidateurCoup
    {
        /// <summary>
        /// Vérifie si un coup est légal (respecte les règles et ne met pas le roi en échec)
        /// </summary>
        /// <param name="coup">Le coup à valider</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le coup est légal</returns>
        bool EstCoupLegal(Coup coup, Echiquier echiquier);

        /// <summary>
        /// Vérifie si un mouvement est valide pour une pièce donnée
        /// </summary>
        /// <param name="piece">La pièce à déplacer</param>
        /// <param name="ligneDestination">Ligne de destination</param>
        /// <param name="colonneDestination">Colonne de destination</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le mouvement est valide</returns>
        bool EstMouvementValide(Piece piece, int ligneDestination, int colonneDestination, Echiquier echiquier);

        /// <summary>
        /// Obtient tous les coups légaux pour une pièce
        /// </summary>
        /// <param name="piece">La pièce</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>Liste des coups légaux</returns>
        List<Coup> ObtenirCoupsLegaux(Piece piece, Echiquier echiquier);

        /// <summary>
        /// Obtient tous les coups légaux pour un joueur
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>Liste de tous les coups légaux</returns>
        List<Coup> ObtenirTousCoupsLegaux(CouleurPiece couleur, Echiquier echiquier);

        /// <summary>
        /// Vérifie si un coup mettrait le roi en échec
        /// </summary>
        /// <param name="coup">Le coup à vérifier</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le coup met le roi en échec</returns>
        bool CoupMetRoiEnEchec(Coup coup, Echiquier echiquier);

        /// <summary>
        /// Valide un roque
        /// </summary>
        /// <param name="coup">Le coup de roque</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si le roque est valide</returns>
        bool ValiderRoque(Coup coup, Echiquier echiquier);

        /// <summary>
        /// Valide une prise en passant
        /// </summary>
        /// <param name="coup">Le coup en passant</param>
        /// <param name="echiquier">L'échiquier actuel</param>
        /// <returns>True si l'en passant est valide</returns>
        bool ValiderEnPassant(Coup coup, Echiquier echiquier);
    }
}