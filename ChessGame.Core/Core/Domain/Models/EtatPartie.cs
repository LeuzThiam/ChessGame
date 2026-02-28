using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Représente l'état complet d'une partie d'échecs
    /// </summary>
    public class EtatPartie
    {
        #region Propriétés

        /// <summary>
        /// Joueur blanc
        /// </summary>
        public Joueur JoueurBlanc { get; set; }

        /// <summary>
        /// Joueur noir
        /// </summary>
        public Joueur JoueurNoir { get; set; }

        /// <summary>
        /// Joueur dont c'est le tour
        /// </summary>
        public Joueur JoueurActif { get; set; }

        /// <summary>
        /// Statut actuel de la partie
        /// </summary>
        public StatutPartie Statut { get; set; }

        /// <summary>
        /// Historique de tous les coups joués
        /// </summary>
        public List<Coup> HistoriqueCoups { get; set; }

        /// <summary>
        /// Dernier coup joué
        /// </summary>
        public Coup? DernierCoup => HistoriqueCoups.LastOrDefault();

        /// <summary>
        /// Numéro du coup actuel
        /// </summary>
        public int NumeroCoup { get; set; }



        /// <summary>
        /// Nombre de demi-coups depuis la dernière capture ou mouvement de pion
        /// (pour la règle des 50 coups)
        /// </summary>
        public int CompteurDemiCoups { get; set; }

        /// <summary>
        /// Date et heure de début de la partie
        /// </summary>
        public DateTime DateDebut { get; set; }

        /// <summary>
        /// Date et heure de fin de la partie
        /// </summary>
        public DateTime? DateFin { get; set; }

        /// <summary>
        /// Durée totale de la partie
        /// </summary>
        public TimeSpan Duree => DateFin.HasValue ? DateFin.Value - DateDebut : DateTime.Now - DateDebut;

        /// <summary>
        /// Type de fin de partie
        /// </summary>
        public TypeFinPartie TypeFin { get; set; }

        /// <summary>
        /// Joueur gagnant (null en cas de nulle ou partie en cours)
        /// </summary>
        public Joueur? Gagnant { get; set; }

        /// <summary>
        /// Historique des positions (notation FEN) pour détecter la répétition triple
        /// </summary>
        public Dictionary<string, int> HistoriquePositions { get; set; }

        /// <summary>
        /// Indique si la partie est terminée
        /// </summary>
        public bool EstTerminee =>
            Statut == StatutPartie.EchecEtMatBlanc ||
            Statut == StatutPartie.EchecEtMatNoir ||
            Statut == StatutPartie.Pat ||
            Statut == StatutPartie.Nulle ||
            Statut == StatutPartie.AbandonBlanc ||
            Statut == StatutPartie.AbandonNoir;

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EtatPartie()
        {
            HistoriqueCoups = new List<Coup>();
            HistoriquePositions = new Dictionary<string, int>();
            Statut = StatutPartie.EnCours;
            NumeroCoup = 1;
            CompteurDemiCoups = 0;
            DateDebut = DateTime.Now;
            TypeFin = TypeFinPartie.Aucune;
            JoueurBlanc = new Joueur("Blancs", CouleurPiece.Blanc);
            JoueurNoir = new Joueur("Noirs", CouleurPiece.Noir);
            JoueurActif = JoueurBlanc;

        }

        /// <summary>
        /// Constructeur avec joueurs
        /// </summary>
        public EtatPartie(Joueur joueurBlanc, Joueur joueurNoir) : this()
        {
            JoueurBlanc = joueurBlanc;
            JoueurNoir = joueurNoir;
            JoueurActif = joueurBlanc; // Les blancs commencent
        }

        #endregion

        #region Méthodes de gestion des coups

        /// <summary>
        /// Ajoute un coup à l'historique
        /// </summary>
        public void AjouterCoup(Coup coup)
        {
            if (coup == null)
                throw new ArgumentNullException(nameof(coup));

            HistoriqueCoups.Add(coup);

            // Incrémenter le compteur de demi-coups
            if (coup.Piece?.Type == TypePiece.Pion || coup.EstCapture())
            {
                CompteurDemiCoups = 0; // Réinitialiser le compteur
            }
            else
            {
                CompteurDemiCoups++;
            }

            // Changer de joueur
            

            // Incrémenter le numéro de coup après le tour des noirs
            if (JoueurActif.Couleur == CouleurPiece.Blanc)
            {
                NumeroCoup++;
            }
        }

        /// <summary>
        /// Annule le dernier coup
        /// </summary>
        public Coup? AnnulerDernierCoup()
        {
            if (HistoriqueCoups.Count == 0)
                return null;

            Coup dernierCoup = HistoriqueCoups[HistoriqueCoups.Count - 1];
            HistoriqueCoups.RemoveAt(HistoriqueCoups.Count - 1);

            // Revenir au joueur précédent
            

            // Décrémenter le numéro de coup si on annule un coup des blancs
            if (JoueurActif.Couleur == CouleurPiece.Blanc)
            {
                NumeroCoup--;
            }

            return dernierCoup;
        }

        #endregion

        #region Méthodes de gestion des joueurs

        /// <summary>
        /// Change le joueur actif
        /// </summary>
        private void ChangerJoueurActif()
        {
            JoueurActif.TerminerTour();
            JoueurActif = JoueurActif == JoueurBlanc ? JoueurNoir : JoueurBlanc;
            JoueurActif.CommencerTour();
        }

        /// <summary>
        /// Obtient le joueur adverse du joueur actif
        /// </summary>
        public Joueur ObtenirJoueurAdverse()
        {
            return JoueurActif == JoueurBlanc ? JoueurNoir : JoueurBlanc;
        }

        /// <summary>
        /// Obtient un joueur par couleur
        /// </summary>
        public Joueur ObtenirJoueurParCouleur(CouleurPiece couleur)
        {
            return couleur == CouleurPiece.Blanc ? JoueurBlanc : JoueurNoir;
        }

        #endregion

        #region Méthodes de vérification des règles

        /// <summary>
        /// Vérifie la règle des 50 coups
        /// </summary>
        public bool EstRegleDes50Coups()
        {
            return CompteurDemiCoups >= 100; // 50 coups complets = 100 demi-coups
        }

        /// <summary>
        /// Ajoute une position à l'historique et vérifie la répétition triple
        /// </summary>
        /// <param name="positionFEN">Position en notation FEN</param>
        /// <returns>True si répétition triple détectée</returns>
        public bool AjouterPosition(string positionFEN)
        {
            if (HistoriquePositions.ContainsKey(positionFEN))
            {
                HistoriquePositions[positionFEN]++;
            }
            else
            {
                HistoriquePositions[positionFEN] = 1;
            }

            return HistoriquePositions[positionFEN] >= 3;
        }

        /// <summary>
        /// Vérifie si la position actuelle est une répétition triple
        /// </summary>
        public bool EstRepetitionTriple(string positionFEN)
        {
            return HistoriquePositions.ContainsKey(positionFEN) &&
                   HistoriquePositions[positionFEN] >= 3;
        }

        #endregion

        #region Méthodes de fin de partie

        /// <summary>
        /// Termine la partie
        /// </summary>
        public void TerminerPartie(StatutPartie statut, TypeFinPartie typeFin, Joueur? gagnant = null)
        {
            Statut = statut;
            TypeFin = typeFin;
            Gagnant = gagnant;
            DateFin = DateTime.Now;
        }

        /// <summary>
        /// Déclare un échec et mat
        /// </summary>
        public void DeclarerEchecEtMat(CouleurPiece couleurPerdante)
        {
            Statut = couleurPerdante == CouleurPiece.Blanc
                ? StatutPartie.EchecEtMatBlanc
                : StatutPartie.EchecEtMatNoir;

            TypeFin = TypeFinPartie.EchecEtMat;
            Gagnant = couleurPerdante == CouleurPiece.Blanc ? JoueurNoir : JoueurBlanc;
            DateFin = DateTime.Now;
        }

        /// <summary>
        /// Déclare un pat (nulle)
        /// </summary>
        public void DeclarerPat()
        {
            Statut = StatutPartie.Pat;
            TypeFin = TypeFinPartie.Pat;
            Gagnant = null;
            DateFin = DateTime.Now;
        }

        /// <summary>
        /// Déclare une nulle
        /// </summary>
        public void DeclarerNulle(TypeFinPartie raison)
        {
            Statut = StatutPartie.Nulle;
            TypeFin = raison;
            Gagnant = null;
            DateFin = DateTime.Now;
        }

        /// <summary>
        /// Enregistre un abandon
        /// </summary>
        public void EnregistrerAbandon(CouleurPiece couleurAbandon)
        {
            Statut = couleurAbandon == CouleurPiece.Blanc
                ? StatutPartie.AbandonBlanc
                : StatutPartie.AbandonNoir;

            TypeFin = TypeFinPartie.Abandon;
            Gagnant = couleurAbandon == CouleurPiece.Blanc ? JoueurNoir : JoueurBlanc;

            var joueurAbandon = ObtenirJoueurParCouleur(couleurAbandon);
            joueurAbandon.Abandonner();

            DateFin = DateTime.Now;
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Réinitialise l'état de la partie
        /// </summary>
        public void Reinitialiser()
        {
            HistoriqueCoups.Clear();
            HistoriquePositions.Clear();
            Statut = StatutPartie.EnCours;
            NumeroCoup = 1;
            CompteurDemiCoups = 0;
            DateDebut = DateTime.Now;
            DateFin = null;
            TypeFin = TypeFinPartie.Aucune;
            Gagnant = null;
            JoueurActif = JoueurBlanc;

            JoueurBlanc.Reinitialiser();
            JoueurNoir.Reinitialiser();
        }

        /// <summary>
        /// Clone l'état de la partie
        /// </summary>
        public EtatPartie Cloner()
        {
            return new EtatPartie
            {
                JoueurBlanc = JoueurBlanc.Cloner(),
                JoueurNoir = JoueurNoir.Cloner(),
                JoueurActif = JoueurActif.Cloner(),
                Statut = this.Statut,
                HistoriqueCoups = new List<Coup>(HistoriqueCoups.Select(c => c.Cloner())),
                NumeroCoup = this.NumeroCoup,
                CompteurDemiCoups = this.CompteurDemiCoups,
                DateDebut = this.DateDebut,
                DateFin = this.DateFin,
                TypeFin = this.TypeFin,
                Gagnant = Gagnant?.Cloner(),
                HistoriquePositions = new Dictionary<string, int>(HistoriquePositions)
            };
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return $"Partie - Coup {NumeroCoup}, {Statut}, Joueur actif: {JoueurActif?.Nom}";
        }

        #endregion
    }
}
