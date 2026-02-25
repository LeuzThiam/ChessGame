using ChessGame.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessGame.Core.Application.Interfaces
{
    /// <summary>
    /// Interface pour la sauvegarde et le chargement de parties d'echecs.
    /// </summary>
    public interface ISauvegardeur
    {
        /// <summary>
        /// Sauvegarde la partie courante au format PGN dans un fichier.
        /// </summary>
        bool SauvegarderPGN(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier);

        /// <summary>
        /// Sauvegarde la partie au format PGN de maniere asynchrone.
        /// </summary>
        Task<bool> SauvegarderPGNAsync(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier PGN.
        /// </summary>
        (Echiquier echiquier, EtatPartie etatPartie)? ChargerPGN(string cheminFichier);

        /// <summary>
        /// Charge une partie depuis un fichier PGN de maniere asynchrone.
        /// </summary>
        Task<(Echiquier echiquier, EtatPartie etatPartie)?> ChargerPGNAsync(string cheminFichier);

        /// <summary>
        /// Exporte la position et l'historique vers une chaine PGN.
        /// </summary>
        string ExporterVersPGN(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Importe une partie a partir d'une chaine PGN.
        /// </summary>
        (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisPGN(string pgnContent);

        /// <summary>
        /// Sauvegarde l'etat de l'echiquier au format FEN dans un fichier.
        /// </summary>
        bool SauvegarderFEN(Echiquier echiquier, string cheminFichier);

        /// <summary>
        /// Charge un echiquier depuis un fichier FEN.
        /// </summary>
        Echiquier ChargerFEN(string cheminFichier);

        /// <summary>
        /// Exporte l'etat de la partie au format FEN.
        /// </summary>
        string ExporterVersFEN(Echiquier echiquier, EtatPartie etatPartie);

        /// <summary>
        /// Importe un echiquier a partir d'une chaine FEN.
        /// </summary>
        Echiquier ImporterDepuisFEN(string fenString);
    }
}
