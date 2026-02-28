using System;

namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Représente un joueur d'échecs
    /// </summary>
    public class Joueur
    {
        #region Propriétés

        /// <summary>
        /// Nom du joueur
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Couleur des pièces du joueur
        /// </summary>
        public CouleurPiece Couleur { get; set; }

        /// <summary>
        /// Indique si c'est le tour du joueur
        /// </summary>
        public bool EstSonTour { get; set; }

        /// <summary>
        /// Temps restant pour le joueur (en secondes)
        /// </summary>
        public TimeSpan TempsRestant { get; set; }

        /// <summary>
        /// Indique si le joueur est en échec
        /// </summary>
        public bool EstEnEchec { get; set; }

        /// <summary>
        /// Nombre de coups joués par le joueur
        /// </summary>
        public int NombreCoups { get; set; }

        /// <summary>
        /// Score du joueur (basé sur le matériel capturé)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Indique si le joueur a abandonné
        /// </summary>
        public bool AAbandon { get; set; }

        /// <summary>
        /// Indique si le joueur peut encore roquer côté roi
        /// </summary>
        public bool PeutRoquerPetit { get; set; }

        /// <summary>
        /// Indique si le joueur peut encore roquer côté dame
        /// </summary>
        public bool PeutRoquerGrand { get; set; }

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Joueur()
        {
            Nom = string.Empty;
            TempsRestant = TimeSpan.FromMinutes(10); // 10 minutes par défaut
            PeutRoquerPetit = true;
            PeutRoquerGrand = true;
        }

        /// <summary>
        /// Constructeur avec nom et couleur
        /// </summary>
        /// <param name="nom">Nom du joueur</param>
        /// <param name="couleur">Couleur des pièces</param>
        public Joueur(string nom, CouleurPiece couleur) : this()
        {
            Nom = nom;
            Couleur = couleur;
            EstSonTour = couleur == CouleurPiece.Blanc; // Les blancs commencent
        }

        /// <summary>
        /// Constructeur avec nom, couleur et temps
        /// </summary>
        /// <param name="nom">Nom du joueur</param>
        /// <param name="couleur">Couleur des pièces</param>
        /// <param name="tempsEnMinutes">Temps initial en minutes</param>
        public Joueur(string nom, CouleurPiece couleur, int tempsEnMinutes) : this(nom, couleur)
        {
            TempsRestant = TimeSpan.FromMinutes(tempsEnMinutes);
        }

        #endregion

        #region Méthodes

        /// <summary>
        /// Commence le tour du joueur
        /// </summary>
        public void CommencerTour()
        {
            EstSonTour = true;
        }

        /// <summary>
        /// Termine le tour du joueur
        /// </summary>
        public void TerminerTour()
        {
            EstSonTour = false;
            NombreCoups++;
        }

        /// <summary>
        /// Ajoute du temps au joueur (incrément)
        /// </summary>
        /// <param name="secondes">Secondes à ajouter</param>
        public void AjouterTemps(int secondes)
        {
            TempsRestant = TempsRestant.Add(TimeSpan.FromSeconds(secondes));
        }

        /// <summary>
        /// Retire du temps au joueur
        /// </summary>
        /// <param name="secondes">Secondes à retirer</param>
        public void RetirerTemps(int secondes)
        {
            TempsRestant = TempsRestant.Subtract(TimeSpan.FromSeconds(secondes));
            if (TempsRestant < TimeSpan.Zero)
                TempsRestant = TimeSpan.Zero;
        }

        /// <summary>
        /// Vérifie si le temps du joueur est écoulé
        /// </summary>
        /// <returns>True si le temps est écoulé</returns>
        public bool EstTempsEcoule()
        {
            return TempsRestant <= TimeSpan.Zero;
        }

        /// <summary>
        /// Réinitialise le joueur pour une nouvelle partie
        /// </summary>
        /// <param name="tempsEnMinutes">Temps initial</param>
        public void Reinitialiser(int tempsEnMinutes = 10)
        {
            EstSonTour = Couleur == CouleurPiece.Blanc;
            EstEnEchec = false;
            AAbandon = false;
            NombreCoups = 0;
            Score = 0;
            TempsRestant = TimeSpan.FromMinutes(tempsEnMinutes);
            PeutRoquerPetit = true;
            PeutRoquerGrand = true;
        }

        /// <summary>
        /// Abandonne la partie
        /// </summary>
        public void Abandonner()
        {
            AAbandon = true;
        }

        /// <summary>
        /// Ajoute des points au score
        /// </summary>
        /// <param name="points">Points à ajouter</param>
        public void AjouterScore(int points)
        {
            Score += points;
        }

        /// <summary>
        /// Clone le joueur
        /// </summary>
        public Joueur Cloner()
        {
            return new Joueur
            {
                Nom = this.Nom,
                Couleur = this.Couleur,
                EstSonTour = this.EstSonTour,
                TempsRestant = this.TempsRestant,
                EstEnEchec = this.EstEnEchec,
                NombreCoups = this.NombreCoups,
                Score = this.Score,
                AAbandon = this.AAbandon,
                PeutRoquerPetit = this.PeutRoquerPetit,
                PeutRoquerGrand = this.PeutRoquerGrand
            };
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return $"{Nom} ({Couleur}) - Score: {Score}, Coups: {NombreCoups}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Joueur autreJoueur)
            {
                return Nom == autreJoueur.Nom && Couleur == autreJoueur.Couleur;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nom, Couleur);
        }

        #endregion
    }
}
