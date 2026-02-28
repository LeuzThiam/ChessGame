using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System.Diagnostics;


namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Service principal de gestion de la partie d'échecs
    /// Implémente IServicePartie : logique de jeu, validation, sauvegarde, stats, événements.
    /// </summary>
    public class ServicePartie : IServicePartie
    {
        private readonly IValidateurCoup _validateurCoup;
        private readonly IReglesJeu _reglesJeu;
        private readonly GenerateurCoups _generateurCoups;
        private readonly IHistoriqueCoups _historique;
        private readonly ISauvegardeur _sauvegardeur;
        private System.Timers.Timer? _timerJeu;
        private readonly IPartieRepository? _partieRepository;



        // Pile pour le "refaire" (redo)
        private readonly Stack<Coup> _coupsAnnules = new();

        // Gestion durée de partie
        private DateTime? _debutPartie;
        private DateTime? _debutSegment;
        private TimeSpan _dureeCumulee = TimeSpan.Zero;

        // Gestion proposition de nulle (côté service)
        private bool _propositionNulleEnCours;

        #region Propriétés

        public Echiquier Echiquier { get; private set; } = null!;
        public EtatPartie EtatPartie { get; private set; } = null!;

        public IHistoriqueCoups Historique => _historique;

        public ISauvegardeur Sauvegardeur => _sauvegardeur;

        #endregion

        #region Événements

        public event EventHandler<Coup>? CoupJoue;
        public event EventHandler<Coup>? CoupAnnule;
        public event EventHandler<StatutPartie>? PartieTerminee;
        public event EventHandler<CouleurPiece>? JoueurEnEchec;
        public event EventHandler<StatutPartie>? StatutPartieChange;

        #endregion

        #region Constructeurs

        /// <summary>
        /// Constructeur avec injection complète.
        /// </summary>
        public ServicePartie(
            IValidateurCoup validateurCoup,
            IReglesJeu reglesJeu,
            IHistoriqueCoups historique,
            ISauvegardeur sauvegardeur,
            IPartieRepository? partieRepository = null)
        {
            _validateurCoup = validateurCoup ?? throw new ArgumentNullException(nameof(validateurCoup));
            _reglesJeu = reglesJeu ?? throw new ArgumentNullException(nameof(reglesJeu));
            _historique = historique ?? throw new ArgumentNullException(nameof(historique));
            _sauvegardeur = sauvegardeur ?? throw new ArgumentNullException(nameof(sauvegardeur));
            _partieRepository = partieRepository;

            _generateurCoups = new GenerateurCoups(_validateurCoup);
        }

        public ServicePartie(IValidateurCoup validateurCoup, IReglesJeu reglesJeu, IHistoriqueCoups historique, ISauvegardeur sauvegardeur)
            : this(validateurCoup, reglesJeu, historique, sauvegardeur, null)
        {
        }

        #endregion

        #region Gestion de la partie

        public void DemarrerNouvellePartie(string nomJoueurBlanc, string nomJoueurNoir, int tempsParJoueur = 10)
        {
            if (string.IsNullOrWhiteSpace(nomJoueurBlanc))
                nomJoueurBlanc = "Blancs";
            if (string.IsNullOrWhiteSpace(nomJoueurNoir))
                nomJoueurNoir = "Noirs";

            Joueur joueurBlanc = new(nomJoueurBlanc, CouleurPiece.Blanc, tempsParJoueur);
            Joueur joueurNoir = new(nomJoueurNoir, CouleurPiece.Noir, tempsParJoueur);

            EtatPartie = new EtatPartie(joueurBlanc, joueurNoir);
            Echiquier = new Echiquier(EtatPartie);
            Echiquier.InitialiserPositionStandard();

            _coupsAnnules.Clear();
            _propositionNulleEnCours = false;

            // Gestion temps de partie
            _debutPartie = DateTime.Now;
            _dureeCumulee = TimeSpan.Zero;
            _debutSegment = _debutPartie;

            // Timer pour gérer le décompte du temps
            _timerJeu = new System.Timers.Timer(1000); // 1 seconde
            _timerJeu.Elapsed += TimerJeu_Tick;
            _timerJeu.AutoReset = true;
            _timerJeu.Start();


        }

        public void ReinitialiserPartie()
        {
            if (EtatPartie == null || Echiquier == null)
                return;

            EtatPartie.Reinitialiser();
            Echiquier.InitialiserPositionStandard();

            _coupsAnnules.Clear();
            _propositionNulleEnCours = false;

            _debutPartie = DateTime.Now;
            _dureeCumulee = TimeSpan.Zero;
            _debutSegment = _debutPartie;

            StatutPartieChange?.Invoke(this, EtatPartie.Statut);
        }

        public void TerminerPartie(StatutPartie statut)
        {
            if (EtatPartie == null)
                return;

            var ancienStatut = EtatPartie.Statut;
            EtatPartie.Statut = statut;

            // On délègue aux méthodes de EtatPartie si elles existent
            switch (statut)
            {
                case StatutPartie.EchecEtMatBlanc:
                    EtatPartie.DeclarerEchecEtMat(CouleurPiece.Blanc);
                    break;
                case StatutPartie.EchecEtMatNoir:
                    EtatPartie.DeclarerEchecEtMat(CouleurPiece.Noir);
                    break;
                case StatutPartie.Pat:
                    EtatPartie.DeclarerPat();
                    break;
                case StatutPartie.Nulle:
                    EtatPartie.DeclarerNulle(TypeFinPartie.Nulle);
                    break;
            }

            // On stoppe la durée
            MettrePause();

            if (ancienStatut != statut)
                StatutPartieChange?.Invoke(this, statut);

            _timerJeu?.Stop();

            PartieTerminee?.Invoke(this, statut);
        }

        public void MettrePause()
        {
            if (_debutSegment.HasValue)
            {
                _dureeCumulee += DateTime.Now - _debutSegment.Value;
                _debutSegment = null;
            }
        }

        public void ReprendrePartie()
        {
            if (_debutPartie.HasValue && !_debutSegment.HasValue)
            {
                _debutSegment = DateTime.Now;
            }
        }

        #endregion

        #region Jouer des coups

        public bool JouerCoup(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee,
                              TypePiece? piecePromotion = null)
        {
            if (EtatPartie == null || Echiquier == null)
                return false;

            if (EtatPartie.EstTerminee)
                return false;

            Piece? piece = Echiquier.ObtenirPiece(ligneDepart, colonneDepart);
            if (piece == null)
                return false;

            if (piece.Couleur != EtatPartie.JoueurActif.Couleur)
                return false;

            Piece? pieceCapturee = Echiquier.ObtenirPiece(ligneArrivee, colonneArrivee);
            Coup coup = new(piece, ligneDepart, colonneDepart, ligneArrivee, colonneArrivee, pieceCapturee);

            DetecterCoupsSpeciaux(coup, piece);

            if (coup.EstPromotion && piecePromotion.HasValue)
            {
                coup.PiecePromotion = piecePromotion.Value;
            }

            // Nouveau coup -> la pile de redo n'est plus valide
            _coupsAnnules.Clear();

            return JouerCoup(coup);
        }

        public bool JouerCoup(Coup coup)
        {
            if (coup == null || EtatPartie == null || Echiquier == null)
                return false;

            // Interdit de jouer si la partie est terminée
            if (EtatPartie.EstTerminee)
                return false;

            // Vérification stricte du tour
            var joueurActif = EtatPartie.JoueurActif;

            if (coup.Piece == null)
                return false;

            if (coup.Piece.Couleur != joueurActif.Couleur)
                return false;

            if (!joueurActif.EstSonTour)
                return false;

            // Vérifier légalité du coup
            if (!_validateurCoup.EstCoupLegal(coup, Echiquier))
                return false;

            // Exécuter le coup
            if (!Echiquier.ExecuterCoup(coup))
                return false;

            // Ajouter à l'historique
            EtatPartie.AjouterCoup(coup);

            // Sauvegarder la position FEN
            string positionFEN = Echiquier.VersNotationFEN();
            EtatPartie.AjouterPosition(positionFEN);

            // Vérifier si l'adversaire est en échec
            CouleurPiece couleurAdverse = EtatPartie.ObtenirJoueurAdverse().Couleur;

            if (_reglesJeu.EstEnEchec(couleurAdverse, Echiquier))
            {
                coup.DonneEchec = true;
                JoueurEnEchec?.Invoke(this, couleurAdverse);

                if (_reglesJeu.EstEchecEtMat(couleurAdverse, Echiquier))
                    coup.DonneEchecEtMat = true;
            }

            // Mise à jour du statut de la partie
            var ancienStatut = EtatPartie.Statut;
            StatutPartie nouveauStatut = _reglesJeu.DeterminerStatutPartie(Echiquier, EtatPartie);
            EtatPartie.Statut = nouveauStatut;

            if (nouveauStatut != ancienStatut)
                StatutPartieChange?.Invoke(this, nouveauStatut);

            // Si la partie est finie
            if (_reglesJeu.EstPartieTerminee(nouveauStatut))
            {
                GererFinPartie(nouveauStatut);
                return true;
            }

            // 🎯 GESTION DES TOURS (IMPORTANT)
            joueurActif.TerminerTour();

            var joueurSuivant = EtatPartie.ObtenirJoueurAdverse();
            joueurSuivant.CommencerTour();

            EtatPartie.JoueurActif = joueurSuivant;

            // Notifier
            CoupJoue?.Invoke(this, coup);

            return true;
        }


        public bool JouerCoupNotation(string notation)
        {
            if (string.IsNullOrWhiteSpace(notation) || EtatPartie == null || Echiquier == null)
                return false;

            notation = notation.Trim();

            // Implémentation simple : forme "e2e4" (coordonnées)
            if (notation.Length == 4 &&
                notation[0] is >= 'a' and <= 'h' &&
                notation[2] is >= 'a' and <= 'h' &&
                notation[1] is >= '1' and <= '8' &&
                notation[3] is >= '1' and <= '8')
            {
                int colFrom = notation[0] - 'a';
                int rowFrom = 8 - (notation[1] - '0');
                int colTo = notation[2] - 'a';
                int rowTo = 8 - (notation[3] - '0');

                return JouerCoup(rowFrom, colFrom, rowTo, colTo);
            }

            // TODO : Parser notation algébrique complète ("e4", "Nf3", "O-O", etc.)
            return false;
        }

        public bool AnnulerCoup()
        {
            if (EtatPartie == null || Echiquier == null)
                return false;

            if (EtatPartie.HistoriqueCoups.Count == 0)
                return false;

            // On récupère le dernier coup pour la pile de redo
            Coup dernier = EtatPartie.HistoriqueCoups.Last();
            _coupsAnnules.Push(dernier);

            // On enlève le coup de l'historique
            EtatPartie.HistoriqueCoups.RemoveAt(EtatPartie.HistoriqueCoups.Count - 1);

            // On reconstruit la partie depuis le début
            RejouerPartieDepuisDebut();

            CoupAnnule?.Invoke(this, dernier);
            return true;
        }

        public bool RefaireCoup()
        {
            if (EtatPartie == null || Echiquier == null)
                return false;

            if (_coupsAnnules.Count == 0)
                return false;

            Coup coup = _coupsAnnules.Pop();

            // On rejoue normalement (ça le remettra dans l'historique)
            return JouerCoup(coup);
        }

        #endregion

        #region Informations sur les coups

        public List<Coup> ObtenirCoupsPossibles(int ligne, int colonne)
        {
            if (EtatPartie == null || Echiquier == null)
                return new List<Coup>();

            Piece? piece = Echiquier.ObtenirPiece(ligne, colonne);
            if (piece == null)
                return new List<Coup>();

            if (piece.Couleur != EtatPartie.JoueurActif.Couleur)
                return new List<Coup>();

            return _validateurCoup.ObtenirCoupsLegaux(piece, Echiquier);
        }

        public List<Coup> ObtenirTousCoupsPossibles()
        {
            var result = new List<Coup>();

            if (EtatPartie == null || Echiquier == null)
                return result;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? p = Echiquier.ObtenirPiece(ligne, colonne);
                    if (p != null && p.Couleur == EtatPartie.JoueurActif.Couleur)
                    {
                        result.AddRange(_validateurCoup.ObtenirCoupsLegaux(p, Echiquier));
                    }
                }
            }

            return result;
        }

        public bool EstCoupValide(int ligneDepart, int colonneDepart, int ligneArrivee, int colonneArrivee)
        {
            if (EtatPartie == null || Echiquier == null)
                return false;

            Piece? piece = Echiquier.ObtenirPiece(ligneDepart, colonneDepart);
            if (piece == null)
                return false;

            Piece? pieceCapturee = Echiquier.ObtenirPiece(ligneArrivee, colonneArrivee);
            Coup coup = new(piece, ligneDepart, colonneDepart, ligneArrivee, colonneArrivee, pieceCapturee);

            DetecterCoupsSpeciaux(coup, piece);

            return _validateurCoup.EstCoupLegal(coup, Echiquier);
        }

        public List<Coup> ObtenirHistorique()
        {
            return EtatPartie?.HistoriqueCoups ?? new List<Coup>();
        }

        #endregion

        #region Informations sur l'échiquier / joueurs

        public Piece? ObtenirPiece(int ligne, int colonne)
        {
            return Echiquier?.ObtenirPiece(ligne, colonne);
        }

        public StatutPartie ObtenirStatutPartie()
        {
            return EtatPartie?.Statut ?? StatutPartie.EnCours;
        }

        public Joueur? ObtenirJoueurActif()
        {
            return EtatPartie?.JoueurActif;
        }

        public Joueur? ObtenirJoueurAdverse()
        {
            return EtatPartie?.ObtenirJoueurAdverse();
        }

        public bool EstPartieTerminee()
        {
            return EtatPartie?.EstTerminee ?? false;
        }

        public bool EstEnEchec(CouleurPiece couleur)
        {
            if (Echiquier == null)
                return false;

            return _reglesJeu.EstEnEchec(couleur, Echiquier);
        }

        #endregion

        #region Fin de partie (nulle, abandon, mat)

        public void ProposerNulle()
        {
            // Ici on se contente de poser un flag côté service.
            // La vue peut s'abonner à cette info pour afficher une boîte de dialogue, etc.
            _propositionNulleEnCours = true;
        }

        public void AccepterNulle()
        {
            if (EtatPartie == null)
                return;

            EtatPartie.DeclarerNulle(TypeFinPartie.Nulle);
            _propositionNulleEnCours = false;

            TerminerPartie(StatutPartie.Nulle);
        }

        public void RefuserNulle()
        {
            _propositionNulleEnCours = false;
        }

        public void Abandonner(CouleurPiece couleur)
        {
            if (EtatPartie == null)
                return;

            EtatPartie.EnregistrerAbandon(couleur);

            var statut = couleur == CouleurPiece.Blanc
                ? StatutPartie.EchecEtMatBlanc   // ou StatutPartie.GagneNoir selon ton enum
                : StatutPartie.EchecEtMatNoir;   // à adapter si tu as un statut spécifique "Abandon"

            TerminerPartie(statut);
        }

        public void DeclarerEchecEtMat(CouleurPiece couleurPerdante)
        {
            if (EtatPartie == null)
                return;

            EtatPartie.DeclarerEchecEtMat(couleurPerdante);

            var statut = couleurPerdante == CouleurPiece.Blanc
                ? StatutPartie.EchecEtMatBlanc
                : StatutPartie.EchecEtMatNoir;

            TerminerPartie(statut);
        }

        #endregion

        #region Sauvegarde / chargement

        public bool SauvegarderPartie(string cheminFichier)
        {
            if (Echiquier == null || EtatPartie == null)
                return false;

            bool success = _sauvegardeur.SauvegarderPGN(Echiquier, EtatPartie, cheminFichier);
            if (!success)
                return false;

            if (_partieRepository != null)
            {
                try
                {
                    string pgn = _sauvegardeur.ExporterVersPGN(Echiquier, EtatPartie);
                    string? fen = Echiquier.VersNotationFEN();
                    _partieRepository.EnregistrerPartie(EtatPartie, pgn, fen);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Impossible d'enregistrer la partie en base : {ex.Message}");
                }
            }

            return true;
        }

        public bool ChargerPartie(string cheminFichier)
        {
            var res = _sauvegardeur.ChargerPGN(cheminFichier);
            if (!res.HasValue)
                return false;

            Echiquier = res.Value.echiquier;
            EtatPartie = res.Value.etatPartie;

            _coupsAnnules.Clear();
            _propositionNulleEnCours = false;

            _debutPartie = DateTime.Now;
            _dureeCumulee = TimeSpan.Zero;
            _debutSegment = _debutPartie;

            StatutPartieChange?.Invoke(this, EtatPartie.Statut);
            return true;
        }

        public bool ChargerPartieDepuisEtat(Echiquier echiquier, EtatPartie etatPartie)
        {
            Echiquier = echiquier;
            EtatPartie = etatPartie;

            _coupsAnnules.Clear();
            _propositionNulleEnCours = false;

            _debutPartie = DateTime.Now;
            _dureeCumulee = TimeSpan.Zero;
            _debutSegment = _debutPartie;

            StatutPartieChange?.Invoke(this, EtatPartie.Statut);
            return true;
        }

        public bool SauvegarderPosition(string cheminFichier)
        {
            if (Echiquier == null)
                return false;

            return _sauvegardeur.SauvegarderFEN(Echiquier, cheminFichier);
        }

        public bool ChargerPosition(string cheminFichier)
        {
            Echiquier nouvelEchiquier = _sauvegardeur.ChargerFEN(cheminFichier);
            if (nouvelEchiquier == null)
                return false;

            // Si aucune partie n'existe, on en crée une simple
            if (EtatPartie == null)
            {
                Joueur blanc = new("Blancs", CouleurPiece.Blanc);
                Joueur noir = new("Noirs", CouleurPiece.Noir);
                EtatPartie = new EtatPartie(blanc, noir);
            }

            Echiquier = nouvelEchiquier;

            EtatPartie.Reinitialiser();
            EtatPartie.Statut = StatutPartie.EnCours;

            _coupsAnnules.Clear();
            _propositionNulleEnCours = false;

            _debutPartie = DateTime.Now;
            _dureeCumulee = TimeSpan.Zero;
            _debutSegment = _debutPartie;

            StatutPartieChange?.Invoke(this, EtatPartie.Statut);
            return true;
        }

        public string ExporterPGN()
        {
            if (Echiquier == null || EtatPartie == null)
                return string.Empty;

            return _sauvegardeur.ExporterVersPGN(Echiquier, EtatPartie);
        }

        public string ExporterFEN()
        {
            if (Echiquier == null || EtatPartie == null)
                return string.Empty;

            return _sauvegardeur.ExporterVersFEN(Echiquier, EtatPartie);
        }

        #endregion

        #region Statistiques

        public StatistiquesHistorique ObtenirStatistiques()
        {
            // Implémentation minimale : on renvoie un objet vide.
            // Tu pourras calculer les vraies stats à partir de EtatPartie.HistoriqueCoups.
            return new StatistiquesHistorique();
        }

        public int CalculerScoreMateriel(CouleurPiece couleur)
        {
            if (Echiquier == null)
                return 0;

            int score = 0;

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? p = Echiquier.ObtenirPiece(ligne, colonne);
                    if (p == null || p.Couleur != couleur)
                        continue;

                    score += p.Type switch
                    {
                        TypePiece.Pion => 1,
                        TypePiece.Cavalier => 3,
                        TypePiece.Fou => 3,
                        TypePiece.Tour => 5,
                        TypePiece.Reine => 9,
                        _ => 0
                    };
                }
            }

            return score;
        }

        public int ObtenirNombreCoups()
        {
            return EtatPartie?.HistoriqueCoups.Count ?? 0;
        }

        public TimeSpan ObtenirDureePartie()
        {
            if (!_debutPartie.HasValue)
                return TimeSpan.Zero;

            TimeSpan total = _dureeCumulee;

            if (_debutSegment.HasValue)
                total += DateTime.Now - _debutSegment.Value;

            return total;
        }

        #endregion

        #region Méthodes privées

        private void DetecterCoupsSpeciaux(Coup coup, Piece piece)
        {
            // Roque
            if (piece is Roi && Math.Abs(coup.ColonneArrivee - coup.ColonneDepart) == 2)
            {
                coup.EstPetitRoque = coup.ColonneArrivee > coup.ColonneDepart;
                coup.EstGrandRoque = coup.ColonneArrivee < coup.ColonneDepart;
            }

            if (piece is Pion pion)
            {
                // Promotion
                coup.EstPromotion = pion.PeutEtrePromu() ||
                    ((pion.Couleur == CouleurPiece.Blanc && coup.LigneArrivee == 0) ||
                     (pion.Couleur == CouleurPiece.Noir && coup.LigneArrivee == 7));

                // En passant
                if (Math.Abs(coup.ColonneArrivee - coup.ColonneDepart) == 1 &&
                    coup.PieceCapturee == null)
                {
                    coup.EstEnPassant = true;
                    coup.PieceCapturee = Echiquier.ObtenirPiece(coup.LigneDepart, coup.ColonneArrivee);
                }
            }
        }

        private void TimerJeu_Tick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (EtatPartie == null)
                return;

            var joueur = EtatPartie.JoueurActif;
            if (joueur == null)
                return;

            joueur.RetirerTemps(1);

            StatutPartieChange?.Invoke(this, EtatPartie.Statut);

            if (joueur.EstTempsEcoule())
            {
                _timerJeu?.Stop();

                var statut = joueur.Couleur == CouleurPiece.Blanc
                    ? StatutPartie.EchecEtMatBlanc
                    : StatutPartie.EchecEtMatNoir;

                TerminerPartie(statut);
            }
        }



        private void GererFinPartie(StatutPartie statut)
        {
            switch (statut)
            {
                case StatutPartie.EchecEtMatBlanc:
                    EtatPartie.DeclarerEchecEtMat(CouleurPiece.Blanc);
                    break;
                case StatutPartie.EchecEtMatNoir:
                    EtatPartie.DeclarerEchecEtMat(CouleurPiece.Noir);
                    break;
                case StatutPartie.Pat:
                    EtatPartie.DeclarerPat();
                    break;
                case StatutPartie.Nulle:
                    if (EtatPartie.EstRegleDes50Coups())
                        EtatPartie.DeclarerNulle(TypeFinPartie.RegleDes50Coups);
                    else if (_reglesJeu.EstMaterielInsuffisant(Echiquier))
                        EtatPartie.DeclarerNulle(TypeFinPartie.MateriellementImpossible);
                    else
                        EtatPartie.DeclarerNulle(TypeFinPartie.RepetitionTriple);
                    break;
            }

            MettrePause();
            PartieTerminee?.Invoke(this, statut);
        }

        /// <summary>
        /// Reconstruit l'échiquier à partir de l'historique courant.
        /// </summary>
        private void RejouerPartieDepuisDebut()
        {
            if (EtatPartie == null || Echiquier == null)
                return;

            var coups = EtatPartie.HistoriqueCoups.ToList();

            Echiquier.InitialiserPositionStandard();
            EtatPartie.HistoriqueCoups.Clear();
            EtatPartie.JoueurActif = EtatPartie.JoueurBlanc;
            EtatPartie.Statut = StatutPartie.EnCours;

            foreach (var coup in coups)
            {
                JouerCoup(coup);
            }
        }

        #endregion
    }
}
