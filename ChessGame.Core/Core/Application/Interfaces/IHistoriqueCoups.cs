using ChessGame.Core.Domain.Models;
using System.Collections.Generic;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour la gestion de l'historique des coups
    /// </summary>
    public interface IHistoriqueCoups
    {
        /// <summary>
        /// Ajoute un coup à l'historique
        /// </summary>
        /// <param name="coup">Le coup à ajouter</param>
        void AjouterCoup(Coup coup);

        /// <summary>
        /// Retire le dernier coup de l'historique
        /// </summary>
        /// <returns>Le coup retiré, ou null si l'historique est vide</returns>
        Coup? RetirerDernierCoup();

        /// <summary>
        /// Obtient le dernier coup joué
        /// </summary>
        /// <returns>Le dernier coup, ou null si l'historique est vide</returns>
        Coup? ObtenirDernierCoup();

        /// <summary>
        /// Obtient tous les coups de l'historique
        /// </summary>
        /// <returns>Liste de tous les coups</returns>
        List<Coup> ObtenirTousLesCoups();

        /// <summary>
        /// Obtient un coup à un index spécifique
        /// </summary>
        /// <param name="index">Index du coup (0-based)</param>
        /// <returns>Le coup à cet index</returns>
        Coup? ObtenirCoup(int index);

        /// <summary>
        /// Obtient le nombre de coups dans l'historique
        /// </summary>
        /// <returns>Nombre de coups</returns>
        int ObtenirNombreCoups();

        /// <summary>
        /// Obtient les coups d'un joueur spécifique
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <returns>Liste des coups du joueur</returns>
        List<Coup> ObtenirCoupsParJoueur(CouleurPiece couleur);

        /// <summary>
        /// Obtient l'historique en notation algébrique
        /// </summary>
        /// <returns>Liste des coups en notation algébrique</returns>
        List<string> ObtenirHistoriqueNotation();

        /// <summary>
        /// Obtient l'historique formaté pour l'affichage
        /// </summary>
        /// <param name="avecNumeros">Inclure les numéros de coups</param>
        /// <returns>Historique formaté</returns>
        string ObtenirHistoriqueFormate(bool avecNumeros = true);

        /// <summary>
        /// Navigue vers un coup spécifique dans l'historique
        /// </summary>
        /// <param name="index">Index du coup</param>
        /// <returns>True si la navigation a réussi</returns>
        bool NaviguerVersCoup(int index);

        /// <summary>
        /// Obtient l'index du coup actuel lors de la navigation
        /// </summary>
        /// <returns>Index actuel</returns>
        int ObtenirIndexActuel();

        /// <summary>
        /// Vérifie si on peut revenir en arrière dans l'historique
        /// </summary>
        /// <returns>True si possible</returns>
        bool PeutReculer();

        /// <summary>
        /// Vérifie si on peut avancer dans l'historique
        /// </summary>
        /// <returns>True si possible</returns>
        bool PeutAvancer();

        /// <summary>
        /// Recule d'un coup dans l'historique
        /// </summary>
        /// <returns>True si le recul a réussi</returns>
        bool Reculer();

        /// <summary>
        /// Avance d'un coup dans l'historique
        /// </summary>
        /// <returns>True si l'avance a réussi</returns>
        bool Avancer();

        /// <summary>
        /// Obtient les statistiques de l'historique
        /// </summary>
        /// <returns>Objet contenant les statistiques</returns>
        StatistiquesHistorique ObtenirStatistiques();

        /// <summary>
        /// Efface tout l'historique
        /// </summary>
        void Effacer();

        /// <summary>
        /// Clone l'historique
        /// </summary>
        /// <returns>Copie de l'historique</returns>
        IHistoriqueCoups Cloner();
    }

    /// <summary>
    /// Statistiques de l'historique des coups
    /// </summary>
    public class StatistiquesHistorique
    {
        /// <summary>
        /// Nombre total de coups
        /// </summary>
        public int NombreTotal { get; set; }

        /// <summary>
        /// Nombre de coups des blancs
        /// </summary>
        public int CoupsBlancs { get; set; }

        /// <summary>
        /// Nombre de coups des noirs
        /// </summary>
        public int CoupsNoirs { get; set; }

        /// <summary>
        /// Nombre de captures
        /// </summary>
        public int NombreCaptures { get; set; }

        /// <summary>
        /// Nombre de coups donnant échec
        /// </summary>
        public int NombreEchecs { get; set; }

        /// <summary>
        /// Nombre de roques
        /// </summary>
        public int NombreRoques { get; set; }

        /// <summary>
        /// Nombre de promotions
        /// </summary>
        public int NombrePromotions { get; set; }

        /// <summary>
        /// Nombre d'en passant
        /// </summary>
        public int NombreEnPassant { get; set; }

        /// <summary>
        /// Coup le plus long (en nombre de cases)
        /// </summary>
        public Coup? CoupLePlusLong { get; set; }

        /// <summary>
        /// Pièce la plus active (nombre de coups)
        /// </summary>
        public TypePiece PieceLaPlusActive { get; set; }
    }
}
