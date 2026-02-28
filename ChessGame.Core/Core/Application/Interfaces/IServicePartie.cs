using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using System;
using System.Collections.Generic;

namespace ChessGame.Core.Application.Interfaces
{
    public interface IServicePartie
    {
        #region Propriétés
        Echiquier Echiquier { get; }
        EtatPartie EtatPartie { get; }
        IHistoriqueCoups Historique { get; }
        ISauvegardeur Sauvegardeur { get; }
        #endregion

        #region Gestion de la partie
        void DemarrerNouvellePartie(string nomJoueurBlanc, string nomJoueurNoir, int tempsParJoueur = 10);
        void ReinitialiserPartie();
        void TerminerPartie(StatutPartie statut);
        void MettrePause();
        void ReprendrePartie();
        #endregion

        #region Jouer des coups
        bool JouerCoup(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee, TypePiece? piecePromotion = null);
        bool JouerCoup(Coup coup);
        bool JouerCoupNotation(string notation);
        bool AnnulerCoup();
        bool RefaireCoup();
        #endregion

        #region Infos coups
        List<Coup> ObtenirCoupsPossibles(int ligne, int colonne);
        List<Coup> ObtenirTousCoupsPossibles();
        bool EstCoupValide(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee);
        List<Coup> ObtenirHistorique();
        #endregion

        #region Infos échiquier / joueurs
        Piece? ObtenirPiece(int ligne, int colonne);
        StatutPartie ObtenirStatutPartie();
        Joueur? ObtenirJoueurActif();
        Joueur? ObtenirJoueurAdverse();
        bool EstPartieTerminee();
        bool EstEnEchec(CouleurPiece couleur);
        #endregion

        #region Fin de partie
        void ProposerNulle();
        void AccepterNulle();
        void RefuserNulle();
        void Abandonner(CouleurPiece couleur);
        void DeclarerEchecEtMat(CouleurPiece couleurPerdante);
        #endregion

        #region Sauvegarde / chargement
        bool SauvegarderPartie(string cheminFichier);
        bool ChargerPartie(string cheminFichier);
        bool ChargerPartieDepuisEtat(Echiquier echiquier, EtatPartie etatPartie);
        bool SauvegarderPosition(string cheminFichier);
        bool ChargerPosition(string cheminFichier);
        string ExporterPGN();
        string ExporterFEN();
        #endregion

        #region Statistiques
        StatistiquesHistorique ObtenirStatistiques();
        int CalculerScoreMateriel(CouleurPiece couleur);
        int ObtenirNombreCoups();
        TimeSpan ObtenirDureePartie();
        #endregion

        #region Événements
        event EventHandler<Coup>? CoupJoue;
        event EventHandler<Coup>? CoupAnnule;
        event EventHandler<StatutPartie>? PartieTerminee;
        event EventHandler<CouleurPiece>? JoueurEnEchec;
        event EventHandler<StatutPartie>? StatutPartieChange;
        #endregion
    }
}
