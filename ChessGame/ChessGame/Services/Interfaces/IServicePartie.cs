using ChessGame.Models;
using ChessGame.Models.Pieces;
using System;
using System.Collections.Generic;

namespace ChessGame.Services.Interfaces
{
    /// <summary>
    /// Interface principale pour le service de gestion de partie d'échecs
    /// Centralise toutes les opérations de jeu, validation, sauvegarde et statistiques
    /// </summary>
    public interface IServicePartie
    {
        #region Propriétés

        /// <summary>
        /// L'échiquier de la partie en cours
        /// </summary>
        Echiquier Echiquier { get; }

        /// <summary>
        /// L'état complet de la partie (joueurs, historique, statut)
        /// </summary>
        EtatPartie EtatPartie { get; }

        /// <summary>
        /// Service de gestion de l'historique des coups
        /// </summary>
        IHistoriqueCoups Historique { get; }

        /// <summary>
        /// Service de sauvegarde et chargement de parties
        /// </summary>
        ISauvegardeur Sauvegardeur { get; }

        #endregion

        #region Gestion de la partie

        /// <summary>
        /// Démarre une nouvelle partie d'échecs
        /// Initialise l'échiquier en position standard et crée les joueurs
        /// </summary>
        /// <param name="nomJoueurBlanc">Nom du joueur avec les pièces blanches</param>
        /// <param name="nomJoueurNoir">Nom du joueur avec les pièces noires</param>
        /// <param name="tempsParJoueur">Temps initial par joueur en minutes (défaut: 10)</param>
        void DemarrerNouvellePartie(string nomJoueurBlanc, string nomJoueurNoir, int tempsParJoueur = 10);

        /// <summary>
        /// Réinitialise la partie en cours
        /// Remet l'échiquier en position initiale et efface l'historique
        /// </summary>
        void ReinitialiserPartie();

        /// <summary>
        /// Termine la partie avec un statut final
        /// </summary>
        /// <param name="statut">Statut final de la partie (échec et mat, nulle, etc.)</param>
        void TerminerPartie(StatutPartie statut);

        /// <summary>
        /// Met la partie en pause (arrête les horloges)
        /// </summary>
        void MettrePause();

        /// <summary>
        /// Reprend la partie après une pause
        /// </summary>
        void ReprendrePartie();

        #endregion

        #region Jouer des coups

        /// <summary>
        /// Joue un coup en spécifiant les coordonnées
        /// </summary>
        /// <param name="ligneDepart">Ligne de départ (0-7)</param>
        /// <param name="colonneDepart">Colonne de départ (0-7)</param>
        /// <param name="ligneArrivee">Ligne d'arrivée (0-7)</param>
        /// <param name="colonneArrivee">Colonne d'arrivée (0-7)</param>
        /// <param name="piecePromotion">Type de pièce pour la promotion du pion (optionnel)</param>
        /// <returns>True si le coup a été joué avec succès, False sinon</returns>
        bool JouerCoup(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee,
                       TypePiece? piecePromotion = null);

        /// <summary>
        /// Joue un coup à partir d'un objet Coup
        /// </summary>
        /// <param name="coup">Le coup à jouer</param>
        /// <returns>True si le coup a été joué avec succès, False sinon</returns>
        bool JouerCoup(Coup coup);

        /// <summary>
        /// Joue un coup en notation algébrique standard
        /// </summary>
        /// <param name="notation">Notation algébrique (ex: "e4", "Nf3", "O-O")</param>
        /// <returns>True si le coup a été joué avec succès, False sinon</returns>
        bool JouerCoupNotation(string notation);

        /// <summary>
        /// Annule le dernier coup joué
        /// Restaure l'état de l'échiquier avant ce coup
        /// </summary>
        /// <returns>True si l'annulation a réussi, False si impossible</returns>
        bool AnnulerCoup();

        /// <summary>
        /// Refait un coup précédemment annulé
        /// Fonctionne uniquement si aucun nouveau coup n'a été joué après l'annulation
        /// </summary>
        /// <returns>True si le refaire a réussi, False sinon</returns>
        bool RefaireCoup();

        #endregion

        #region Obtenir des informations sur les coups

        /// <summary>
        /// Obtient tous les coups légaux possibles pour une pièce à une position donnée
        /// </summary>
        /// <param name="ligne">Ligne de la pièce (0-7)</param>
        /// <param name="colonne">Colonne de la pièce (0-7)</param>
        /// <returns>Liste des coups légaux pour cette pièce</returns>
        List<Coup> ObtenirCoupsPossibles(int ligne, int colonne);

        /// <summary>
        /// Obtient tous les coups légaux possibles pour le joueur actif
        /// </summary>
        /// <returns>Liste complète de tous les coups légaux disponibles</returns>
        List<Coup> ObtenirTousCoupsPossibles();

        /// <summary>
        /// Vérifie si un coup spécifique est valide/légal
        /// </summary>
        /// <param name="ligneDepart">Ligne de départ</param>
        /// <param name="colonneDepart">Colonne de départ</param>
        /// <param name="ligneArrivee">Ligne d'arrivée</param>
        /// <param name="colonneArrivee">Colonne d'arrivée</param>
        /// <returns>True si le coup est valide et légal</returns>
        bool EstCoupValide(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee);

        /// <summary>
        /// Obtient l'historique complet de tous les coups joués
        /// </summary>
        /// <returns>Liste ordonnée des coups depuis le début de la partie</returns>
        List<Coup> ObtenirHistorique();

        #endregion

        #region Informations sur l'échiquier et les pièces

        /// <summary>
        /// Obtient la pièce présente à une position donnée
        /// </summary>
        /// <param name="ligne">Ligne (0-7)</param>
        /// <param name="colonne">Colonne (0-7)</param>
        /// <returns>La pièce à cette position, ou null si la case est vide</returns>
        Piece ObtenirPiece(int ligne, int colonne);

        /// <summary>
        /// Obtient le statut actuel de la partie
        /// </summary>
        /// <returns>Le statut (en cours, échec, échec et mat, pat, nulle, etc.)</returns>
        StatutPartie ObtenirStatutPartie();

        /// <summary>
        /// Obtient le joueur dont c'est actuellement le tour
        /// </summary>
        /// <returns>Le joueur actif</returns>
        Joueur ObtenirJoueurActif();

        /// <summary>
        /// Obtient le joueur adverse du joueur actif
        /// </summary>
        /// <returns>Le joueur adverse</returns>
        Joueur ObtenirJoueurAdverse();

        /// <summary>
        /// Vérifie si la partie est terminée
        /// </summary>
        /// <returns>True si la partie est terminée (mat, pat, abandon, etc.)</returns>
        bool EstPartieTerminee();

        /// <summary>
        /// Vérifie si un joueur spécifique est en échec
        /// </summary>
        /// <param name="couleur">Couleur du joueur à vérifier</param>
        /// <returns>True si le joueur est en échec</returns>
        bool EstEnEchec(CouleurPiece couleur);

        #endregion

        #region Fin de partie

        /// <summary>
        /// Propose une partie nulle à l'adversaire
        /// L'adversaire doit accepter ou refuser avec AccepterNulle() ou RefuserNulle()
        /// </summary>
        void ProposerNulle();

        /// <summary>
        /// Accepte une proposition de partie nulle
        /// Termine la partie avec le statut Nulle
        /// </summary>
        void AccepterNulle();

        /// <summary>
        /// Refuse une proposition de partie nulle
        /// La partie continue normalement
        /// </summary>
        void RefuserNulle();

        /// <summary>
        /// Un joueur abandonne la partie
        /// L'adversaire gagne automatiquement
        /// </summary>
        /// <param name="couleur">Couleur du joueur qui abandonne</param>
        void Abandonner(CouleurPiece couleur);

        /// <summary>
        /// Déclare un échec et mat
        /// Utilisé par le système de règles pour terminer la partie
        /// </summary>
        /// <param name="couleurPerdante">Couleur du joueur mis échec et mat</param>
        void DeclarerEchecEtMat(CouleurPiece couleurPerdante);

        #endregion

        #region Sauvegarde et chargement

        /// <summary>
        /// Sauvegarde la partie actuelle au format PGN
        /// </summary>
        /// <param name="cheminFichier">Chemin complet du fichier de sauvegarde</param>
        /// <returns>True si la sauvegarde a réussi</returns>
        bool SauvegarderPartie(string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier PGN
        /// Remplace la partie en cours
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier PGN à charger</param>
        /// <returns>True si le chargement a réussi</returns>
        bool ChargerPartie(string cheminFichier);

        /// <summary>
        /// Sauvegarde uniquement la position actuelle au format FEN
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier FEN</param>
        /// <returns>True si la sauvegarde a réussi</returns>
        bool SauvegarderPosition(string cheminFichier);

        /// <summary>
        /// Charge une position depuis un fichier FEN
        /// Crée une nouvelle partie à partir de cette position
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier FEN</param>
        /// <returns>True si le chargement a réussi</returns>
        bool ChargerPosition(string cheminFichier);

        /// <summary>
        /// Exporte la partie en cours au format PGN (string)
        /// </summary>
        /// <returns>Contenu PGN sous forme de chaîne</returns>
        string ExporterPGN();

        /// <summary>
        /// Exporte la position actuelle en notation FEN (string)
        /// </summary>
        /// <returns>Notation FEN complète</returns>
        string ExporterFEN();

        #endregion

        #region Statistiques

        /// <summary>
        /// Obtient les statistiques détaillées de la partie
        /// (nombre de coups, captures, échecs, etc.)
        /// </summary>
        /// <returns>Objet contenant toutes les statistiques</returns>
        StatistiquesHistorique ObtenirStatistiques();

        /// <summary>
        /// Calcule le score matériel d'un joueur
        /// Basé sur la valeur des pièces restantes
        /// </summary>
        /// <param name="couleur">Couleur du joueur</param>
        /// <returns>Score matériel total</returns>
        int CalculerScoreMateriel(CouleurPiece couleur);

        /// <summary>
        /// Obtient le nombre total de coups joués
        /// </summary>
        /// <returns>Nombre de coups (demi-coups)</returns>
        int ObtenirNombreCoups();

        /// <summary>
        /// Obtient la durée écoulée depuis le début de la partie
        /// </summary>
        /// <returns>Durée de la partie</returns>
        TimeSpan ObtenirDureePartie();

        #endregion

        #region Événements

        /// <summary>
        /// Événement déclenché lorsqu'un coup est joué avec succès
        /// </summary>
        event EventHandler<Coup> CoupJoue;

        /// <summary>
        /// Événement déclenché lorsqu'un coup est annulé
        /// </summary>
        event EventHandler<Coup> CoupAnnule;

        /// <summary>
        /// Événement déclenché lorsque la partie se termine
        /// </summary>
        event EventHandler<StatutPartie> PartieTerminee;

        /// <summary>
        /// Événement déclenché lorsqu'un joueur est mis en échec
        /// </summary>
        event EventHandler<CouleurPiece> JoueurEnEchec;

        /// <summary>
        /// Événement déclenché lorsque le statut de la partie change
        /// (en cours -> échec, échec -> mat, etc.)
        /// </summary>
        event EventHandler<StatutPartie> StatutPartieChange;

        #endregion
    }
}