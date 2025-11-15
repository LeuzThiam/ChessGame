using ChessGame.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Services.Interfaces
{
    /// <summary>
    /// Interface pour la sauvegarde et le chargement de parties d'échecs
    /// Supporte les formats PGN, FEN et JSON
    /// </summary>
    public interface ISauvegardeur
    {
        #region Sauvegarde/Chargement PGN

        /// <summary>
        /// Sauvegarde une partie au format PGN (Portable Game Notation)
        /// </summary>
        /// <param name="echiquier">L'échiquier à sauvegarder</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <param name="cheminFichier">Chemin complet du fichier de sauvegarde</param>
        /// <returns>True si la sauvegarde a réussi, False sinon</returns>
        bool SauvegarderPGN(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier);

        /// <summary>
        /// Sauvegarde une partie au format PGN de manière asynchrone
        /// </summary>
        /// <param name="echiquier">L'échiquier à sauvegarder</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <param name="cheminFichier">Chemin complet du fichier de sauvegarde</param>
        /// <returns>Task retournant True si la sauvegarde a réussi</returns>
        Task<bool> SauvegarderPGNAsync(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier PGN
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier PGN à charger</param>
        /// <returns>Tuple (échiquier, état) si succès, null si échec</returns>
        (Echiquier echiquier, EtatPartie etatPartie)? ChargerPGN(string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier PGN de manière asynchrone
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier PGN à charger</param>
        /// <returns>Task retournant le tuple (échiquier, état) ou null</returns>
        Task<(Echiquier echiquier, EtatPartie etatPartie)?> ChargerPGNAsync(string cheminFichier);

        /// <summary>
        /// Exporte une partie au format PGN sous forme de string
        /// </summary>
        /// <param name="echiquier">L'échiquier</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <returns>Contenu PGN sous forme de chaîne de caractères</returns>
        string ExporterVersPGN(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Importe une partie depuis une string au format PGN
        /// </summary>
        /// <param name="pgnContent">Contenu PGN</param>
        /// <returns>Tuple (échiquier, état) si succès, null si échec</returns>
        (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisPGN(string pgnContent);

        #endregion

        #region Sauvegarde/Chargement FEN

        /// <summary>
        /// Sauvegarde la position actuelle au format FEN (Forsyth-Edwards Notation)
        /// </summary>
        /// <param name="echiquier">L'échiquier à sauvegarder</param>
        /// <param name="cheminFichier">Chemin du fichier</param>
        /// <returns>True si la sauvegarde a réussi</returns>
        bool SauvegarderFEN(Echiquier echiquier, string cheminFichier);

        /// <summary>
        /// Charge une position depuis un fichier FEN
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier FEN</param>
        /// <returns>L'échiquier chargé, ou null en cas d'échec</returns>
        Echiquier ChargerFEN(string cheminFichier);

        /// <summary>
        /// Exporte la position actuelle en notation FEN complète
        /// </summary>
        /// <param name="echiquier">L'échiquier</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <returns>String FEN complète (position + métadonnées)</returns>
        string ExporterVersFEN(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Importe une position depuis une notation FEN
        /// </summary>
        /// <param name="fenString">String FEN à importer</param>
        /// <returns>L'échiquier créé, ou null en cas d'échec</returns>
        Echiquier ImporterDepuisFEN(string fenString);

        #endregion

        #region Sauvegarde/Chargement JSON

        /// <summary>
        /// Sauvegarde une partie au format JSON (format personnalisé)
        /// </summary>
        /// <param name="echiquier">L'échiquier</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <param name="cheminFichier">Chemin du fichier</param>
        /// <returns>True si la sauvegarde a réussi</returns>
        bool SauvegarderJSON(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier JSON
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier JSON</param>
        /// <returns>Tuple (échiquier, état) si succès, null si échec</returns>
        (Echiquier echiquier, EtatPartie etatPartie)? ChargerJSON(string cheminFichier);

        /// <summary>
        /// Exporte une partie au format JSON sous forme de string
        /// </summary>
        /// <param name="echiquier">L'échiquier</param>
        /// <param name="etatPartie">L'état de la partie</param>
        /// <returns>Contenu JSON</returns>
        string ExporterVersJSON(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Importe une partie depuis une string JSON
        /// </summary>
        /// <param name="jsonContent">Contenu JSON</param>
        /// <returns>Tuple (échiquier, état) si succès, null si échec</returns>
        (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisJSON(string jsonContent);

        #endregion

        #region Métadonnées

        /// <summary>
        /// Obtient les métadonnées d'une partie sans la charger complètement
        /// Lecture rapide des informations essentielles (joueurs, date, résultat...)
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier</param>
        /// <returns>Métadonnées de la partie, ou null si erreur</returns>
        MetadonneesPartie ObtenirMetadonnees(string cheminFichier);

        /// <summary>
        /// Liste toutes les parties sauvegardées dans un dossier
        /// </summary>
        /// <param name="cheminDossier">Chemin du dossier à scanner</param>
        /// <returns>Liste des métadonnées de toutes les parties trouvées</returns>
        List<MetadonneesPartie> ListerPartiesSauvegardees(string cheminDossier);

        /// <summary>
        /// Liste les parties sauvegardées avec filtres avancés
        /// </summary>
        /// <param name="cheminDossier">Chemin du dossier</param>
        /// <param name="format">Format à filtrer (PGN, FEN, JSON), null = tous</param>
        /// <param name="dateDebut">Date minimale (inclusive)</param>
        /// <param name="dateFin">Date maximale (inclusive)</param>
        /// <returns>Liste filtrée des métadonnées</returns>
        List<MetadonneesPartie> ListerPartiesSauvegardeesAvecFiltres(
            string cheminDossier,
            string format = null,
            DateTime? dateDebut = null,
            DateTime? dateFin = null);

        /// <summary>
        /// Recherche des parties contenant un joueur spécifique
        /// </summary>
        /// <param name="cheminDossier">Chemin du dossier</param>
        /// <param name="nomJoueur">Nom du joueur à rechercher (recherche partielle)</param>
        /// <returns>Liste des parties contenant ce joueur</returns>
        List<MetadonneesPartie> RechercherPartiesParJoueur(string cheminDossier, string nomJoueur);

        #endregion

        #region Validation

        /// <summary>
        /// Valide un fichier PGN (vérifie les en-têtes obligatoires)
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier à valider</param>
        /// <returns>True si le fichier PGN est valide</returns>
        bool ValiderFichierPGN(string cheminFichier);

        /// <summary>
        /// Valide une string FEN (vérifie la syntaxe)
        /// </summary>
        /// <param name="fenString">String FEN à valider</param>
        /// <returns>True si la notation FEN est valide</returns>
        bool ValiderFEN(string fenString);

        /// <summary>
        /// Valide un fichier JSON de partie
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier à valider</param>
        /// <returns>True si le fichier JSON est valide</returns>
        bool ValiderFichierJSON(string cheminFichier);

        #endregion

        #region Utilitaires de gestion de fichiers

        /// <summary>
        /// Supprime une partie sauvegardée
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier à supprimer</param>
        /// <returns>True si la suppression a réussi</returns>
        bool SupprimerPartie(string cheminFichier);

        /// <summary>
        /// Renomme/déplace une partie sauvegardée
        /// </summary>
        /// <param name="ancienChemin">Chemin actuel du fichier</param>
        /// <param name="nouveauChemin">Nouveau chemin du fichier</param>
        /// <returns>True si le renommage a réussi</returns>
        bool RenommerPartie(string ancienChemin, string nouveauChemin);

        /// <summary>
        /// Copie une partie sauvegardée
        /// </summary>
        /// <param name="cheminSource">Chemin du fichier source</param>
        /// <param name="cheminDestination">Chemin du fichier destination</param>
        /// <returns>True si la copie a réussi</returns>
        bool CopierPartie(string cheminSource, string cheminDestination);

        /// <summary>
        /// Obtient la taille d'un fichier de partie en octets
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier</param>
        /// <returns>Taille en octets, 0 si fichier inexistant</returns>
        long ObtenirTailleFichier(string cheminFichier);

        /// <summary>
        /// Vérifie si un fichier de partie existe
        /// </summary>
        /// <param name="cheminFichier">Chemin du fichier</param>
        /// <returns>True si le fichier existe</returns>
        bool FichierExiste(string cheminFichier);

        /// <summary>
        /// Obtient l'extension de fichier recommandée pour un format
        /// </summary>
        /// <param name="format">Format (PGN, FEN, JSON)</param>
        /// <returns>Extension avec le point (ex: ".pgn")</returns>
        string ObtenirExtension(string format);

        #endregion

        #region Sauvegarde automatique

        /// <summary>
        /// Active la sauvegarde automatique périodique
        /// </summary>
        /// <param name="dossier">Dossier où sauvegarder automatiquement</param>
        /// <param name="intervalleMinutes">Intervalle en minutes entre les sauvegardes (défaut: 5)</param>
        void ActiverSauvegardeAutomatique(string dossier, int intervalleMinutes = 5);

        /// <summary>
        /// Désactive la sauvegarde automatique
        /// </summary>
        void DesactiverSauvegardeAutomatique();

        /// <summary>
        /// Vérifie si la sauvegarde automatique est actuellement active
        /// </summary>
        /// <returns>True si la sauvegarde automatique est active</returns>
        bool EstSauvegardeAutomatiqueActive();

        #endregion
    }

    /// <summary>
    /// Classe contenant les métadonnées d'une partie sauvegardée
    /// Permet d'afficher les informations sans charger toute la partie
    /// </summary>
    public class MetadonneesPartie
    {
        #region Informations du fichier

        /// <summary>
        /// Nom du fichier (sans le chemin)
        /// </summary>
        public string NomFichier { get; set; }

        /// <summary>
        /// Chemin complet du fichier
        /// </summary>
        public string CheminComplet { get; set; }

        /// <summary>
        /// Format du fichier (PGN, FEN, JSON)
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Taille du fichier en octets
        /// </summary>
        public long TailleFichier { get; set; }

        /// <summary>
        /// Date de dernière sauvegarde du fichier
        /// </summary>
        public DateTime DateSauvegarde { get; set; }

        #endregion

        #region Informations de la partie

        /// <summary>
        /// Date à laquelle la partie a été jouée
        /// </summary>
        public DateTime DatePartie { get; set; }

        /// <summary>
        /// Nom du joueur avec les pièces blanches
        /// </summary>
        public string JoueurBlanc { get; set; }

        /// <summary>
        /// Nom du joueur avec les pièces noires
        /// </summary>
        public string JoueurNoir { get; set; }

        /// <summary>
        /// Résultat de la partie
        /// "1-0" = victoire des blancs
        /// "0-1" = victoire des noirs
        /// "1/2-1/2" = nulle
        /// "*" = partie en cours ou non terminée
        /// </summary>
        public string Resultat { get; set; }

        /// <summary>
        /// Nombre total de coups joués dans la partie
        /// </summary>
        public int NombreCoups { get; set; }

        /// <summary>
        /// Durée totale de la partie (si disponible)
        /// </summary>
        public TimeSpan? DureePartie { get; set; }

        #endregion

        #region Informations supplémentaires (PGN)

        /// <summary>
        /// Nom de l'événement ou du tournoi
        /// </summary>
        public string Evenement { get; set; }

        /// <summary>
        /// Lieu où la partie a été jouée
        /// </summary>
        public string Lieu { get; set; }

        /// <summary>
        /// ELO du joueur blanc (si disponible)
        /// </summary>
        public int? EloJoueurBlanc { get; set; }

        /// <summary>
        /// ELO du joueur noir (si disponible)
        /// </summary>
        public int? EloJoueurNoir { get; set; }

        #endregion

        #region Aperçu des coups

        /// <summary>
        /// Notation du premier coup de la partie (ex: "e4")
        /// </summary>
        public string PremierCoup { get; set; }

        /// <summary>
        /// Notation du dernier coup de la partie
        /// </summary>
        public string DernierCoup { get; set; }

        #endregion

        #region Métadonnées personnalisées

        /// <summary>
        /// Description ou notes sur la partie
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tags personnalisés pour organiser les parties
        /// Exemples: "Tactique", "Finale", "Ouverture italienne", etc.
        /// </summary>
        public List<string> Tags { get; set; }

        #endregion

        #region Constructeur

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public MetadonneesPartie()
        {
            Tags = new List<string>();
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Représentation textuelle des métadonnées
        /// </summary>
        /// <returns>String formatée avec les informations principales</returns>
        public override string ToString()
        {
            return $"{JoueurBlanc} vs {JoueurNoir} ({DatePartie:dd/MM/yyyy}) - {Resultat}";
        }

        /// <summary>
        /// Obtient une description complète de la partie
        /// </summary>
        /// <returns>Description détaillée</returns>
        public string ObtenirDescriptionComplete()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Partie: {JoueurBlanc} vs {JoueurNoir}");
            sb.AppendLine($"Date: {DatePartie:dd/MM/yyyy}");
            sb.AppendLine($"Résultat: {Resultat}");
            sb.AppendLine($"Coups: {NombreCoups}");

            if (DureePartie.HasValue)
                sb.AppendLine($"Durée: {DureePartie.Value.TotalMinutes:F1} minutes");

            if (!string.IsNullOrEmpty(Evenement))
                sb.AppendLine($"Événement: {Evenement}");

            if (!string.IsNullOrEmpty(Lieu))
                sb.AppendLine($"Lieu: {Lieu}");

            return sb.ToString();
        }

        #endregion
    }
}