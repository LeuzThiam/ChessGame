using ChessGame.Core.Domain.Models.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Représente l'échiquier complet avec toutes les pièces
    /// </summary>
    public class Echiquier
    {
        #region Propriétés

        /// <summary>
        /// Grille de cases (8x8)
        /// </summary>
        public Case[,] Cases { get; private set; }

        /// <summary>
        /// État de la partie
        /// </summary>
        public EtatPartie? EtatPartie { get; set; }

        /// <summary>
        /// Liste de toutes les pièces blanches sur l'échiquier
        /// </summary>
        public List<Piece> PiecesBlanches => ObtenirPieces(CouleurPiece.Blanc);

        /// <summary>
        /// Liste de toutes les pièces noires sur l'échiquier
        /// </summary>
        public List<Piece> PiecesNoires => ObtenirPieces(CouleurPiece.Noir);

        /// <summary>
        /// Roi blanc
        /// </summary>
        public Roi? RoiBlanc { get; private set; }

        /// <summary>
        /// Roi noir
        /// </summary>
        public Roi? RoiNoir { get; private set; }

        #endregion

        #region Constructeur

        /// <summary>
        /// Constructeur - Initialise l'échiquier vide
        /// </summary>
        public Echiquier()
        {
            Cases = new Case[8, 8];
            InitialiserCases();

            // Ne surtout pas instancier ici !
            EtatPartie = null;

            RoiBlanc = null;
            RoiNoir = null;
        }


        /// <summary>
        /// Constructeur avec état de partie
        /// </summary>
        public Echiquier(EtatPartie? etatPartie) : this()
        {
            EtatPartie = etatPartie;
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Initialise toutes les cases de l'échiquier
        /// </summary>
        private void InitialiserCases()
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Cases[ligne, colonne] = new Case(ligne, colonne);
                }
            }
        }

        /// <summary>
        /// Positionne les pièces pour une partie standard
        /// </summary>
        public void InitialiserPositionStandard()
        {
            // Vider l'échiquier
            ViderEchiquier();

            // Pièces noires (ligne 0 et 1)
            PlacerPiece(new Tour(CouleurPiece.Noir, 0, 0), 0, 0);
            PlacerPiece(new Cavalier(CouleurPiece.Noir, 0, 1), 0, 1);
            PlacerPiece(new Fou(CouleurPiece.Noir, 0, 2), 0, 2);
            PlacerPiece(new Reine(CouleurPiece.Noir, 0, 3), 0, 3);
            RoiNoir = new Roi(CouleurPiece.Noir, 0, 4);
            PlacerPiece(RoiNoir, 0, 4);
            PlacerPiece(new Fou(CouleurPiece.Noir, 0, 5), 0, 5);
            PlacerPiece(new Cavalier(CouleurPiece.Noir, 0, 6), 0, 6);
            PlacerPiece(new Tour(CouleurPiece.Noir, 0, 7), 0, 7);

            // Pions noirs
            for (int col = 0; col < 8; col++)
            {
                PlacerPiece(new Pion(CouleurPiece.Noir, 1, col), 1, col);
            }

            // Pions blancs
            for (int col = 0; col < 8; col++)
            {
                PlacerPiece(new Pion(CouleurPiece.Blanc, 6, col), 6, col);
            }

            // Pièces blanches (ligne 7 et 6)
            PlacerPiece(new Tour(CouleurPiece.Blanc, 7, 0), 7, 0);
            PlacerPiece(new Cavalier(CouleurPiece.Blanc, 7, 1), 7, 1);
            PlacerPiece(new Fou(CouleurPiece.Blanc, 7, 2), 7, 2);
            PlacerPiece(new Reine(CouleurPiece.Blanc, 7, 3), 7, 3);
            RoiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            PlacerPiece(RoiBlanc, 7, 4);
            PlacerPiece(new Fou(CouleurPiece.Blanc, 7, 5), 7, 5);
            PlacerPiece(new Cavalier(CouleurPiece.Blanc, 7, 6), 7, 6);
            PlacerPiece(new Tour(CouleurPiece.Blanc, 7, 7), 7, 7);
        }

        /// <summary>
        /// Vide complètement l'échiquier
        /// </summary>
        public void ViderEchiquier()
        {
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Cases[ligne, colonne].RetirerPiece();
                }
            }
            RoiBlanc = null;
            RoiNoir = null;
        }

        #endregion

        #region Gestion des pièces

        /// <summary>
        /// Place une pièce sur l'échiquier
        /// </summary>
        public void PlacerPiece(Piece piece, int ligne, int colonne)
        {
            if (piece == null)
                throw new ArgumentNullException(nameof(piece));

            if (!EstPositionValide(ligne, colonne))
                throw new ArgumentException($"Position invalide: ({ligne}, {colonne})");

            Cases[ligne, colonne].PlacerPiece(piece);
            piece.Ligne = ligne;
            piece.Colonne = colonne;
            piece.ADejaBougee = false; // IMPORTANT

        }

        /// <summary>
        /// Obtient la pièce à une position donnée
        /// </summary>
        public Piece? ObtenirPiece(int ligne, int colonne)
        {
            if (!EstPositionValide(ligne, colonne))
                return null;

            return Cases[ligne, colonne].Piece;
        }

        /// <summary>
        /// Obtient toutes les pièces d'une couleur
        /// </summary>
        public List<Piece> ObtenirPieces(CouleurPiece couleur)
        {
            List<Piece> pieces = new List<Piece>();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? piece = Cases[ligne, colonne].Piece;
                    if (piece != null && piece.Couleur == couleur)
                    {
                        pieces.Add(piece);
                    }
                }
            }

            return pieces;
        }

        /// <summary>
        /// Trouve le roi d'une couleur donnée
        /// </summary>
        public Roi? TrouverRoi(CouleurPiece couleur)
        {
            return couleur == CouleurPiece.Blanc ? RoiBlanc : RoiNoir;
        }

        #endregion

        #region Mouvements

        /// <summary>
        /// Exécute un coup sur l'échiquier
        /// </summary>
        public bool ExecuterCoup(Coup coup)
        {
            if (coup == null)
                return false;

            Piece? piece = ObtenirPiece(coup.LigneDepart, coup.ColonneDepart);
            if (piece == null)
                return false;

            // -------------------------
            // 1) GESTION DES COUPS SPÉCIAUX
            // -------------------------
            if (coup.EstPetitRoque || coup.EstGrandRoque)
                return ExecuterRoque(coup);

            if (coup.EstEnPassant)
                return ExecuterEnPassant(coup);

            // -------------------------
            // 2) MOUVEMENT NORMAL
            // -------------------------
            Cases[coup.LigneDepart, coup.ColonneDepart].RetirerPiece();
            Cases[coup.LigneArrivee, coup.ColonneArrivee].PlacerPiece(piece);

            // Mise à jour des coordonnées
            piece.Ligne = coup.LigneArrivee;
            piece.Colonne = coup.ColonneArrivee;

            // La pièce a maintenant bougé
            piece.ADejaBougee = true;

            // -------------------------
            // 3) PROMOTION
            // -------------------------
            if (coup.EstPromotion)
                ExecuterPromotion(coup);

            // -------------------------
            // 4) NOTE IMPORTANTE !
            // Ici on NE MET PAS dernier coup.
            // DernierCoup se met automatiquement 
            // quand ServicePartie -> EtatPartie.AjouterCoup(coup)
            // -------------------------

            return true;
        }



        /// <summary>
        /// Exécute le roque
        /// </summary>
        private bool ExecuterRoque(Coup coup)
        {
            Roi? roi = ObtenirPiece(coup.LigneDepart, coup.ColonneDepart) as Roi;
            if (roi == null)
                return false;

            int ligne = coup.LigneDepart;

            if (coup.EstPetitRoque)
            {
                // Déplacer le roi
                Cases[ligne, 4].RetirerPiece();
                Cases[ligne, 6].PlacerPiece(roi);
                roi.Ligne = ligne;
                roi.Colonne = 6;

                // Déplacer la tour
                Tour? tour = ObtenirPiece(ligne, 7) as Tour;
                if (tour != null)
                {
                    Cases[ligne, 7].RetirerPiece();
                    Cases[ligne, 5].PlacerPiece(tour);
                    tour.Ligne = ligne;
                    tour.Colonne = 5;
                    tour.ADejaBougee = true;
                }
            }
            else // Grand roque
            {
                // Déplacer le roi
                Cases[ligne, 4].RetirerPiece();
                Cases[ligne, 2].PlacerPiece(roi);
                roi.Ligne = ligne;
                roi.Colonne = 2;

                // Déplacer la tour
                Tour? tour = ObtenirPiece(ligne, 0) as Tour;
                if (tour != null)
                {
                    Cases[ligne, 0].RetirerPiece();
                    Cases[ligne, 3].PlacerPiece(tour);
                    tour.Ligne = ligne;
                    tour.Colonne = 3;
                    tour.ADejaBougee = true;
                }
            }

            roi.ADejaBougee = true;
            return true;
        }

        /// <summary>
        /// Exécute une prise en passant
        /// </summary>
        private bool ExecuterEnPassant(Coup coup)
        {
            Pion? pion = ObtenirPiece(coup.LigneDepart, coup.ColonneDepart) as Pion;
            if (pion == null)
                return false;

            // Déplacer le pion
            Cases[coup.LigneDepart, coup.ColonneDepart].RetirerPiece();
            Cases[coup.LigneArrivee, coup.ColonneArrivee].PlacerPiece(pion);
            pion.Ligne = coup.LigneArrivee;
            pion.Colonne = coup.ColonneArrivee;
            pion.ADejaBougee = true;

            // Retirer le pion capturé (sur la même ligne que le pion qui capture)
            Cases[coup.LigneDepart, coup.ColonneArrivee].RetirerPiece();

            return true;
        }

        /// <summary>
        /// Exécute la promotion d'un pion
        /// </summary>
        private void ExecuterPromotion(Coup coup)
        {
            if (coup.Piece == null)
                return;

            Piece nouvellePiece = coup.PiecePromotion switch
            {
                TypePiece.Reine => new Reine(coup.Piece.Couleur, coup.LigneArrivee, coup.ColonneArrivee),
                TypePiece.Tour => new Tour(coup.Piece.Couleur, coup.LigneArrivee, coup.ColonneArrivee),
                TypePiece.Fou => new Fou(coup.Piece.Couleur, coup.LigneArrivee, coup.ColonneArrivee),
                TypePiece.Cavalier => new Cavalier(coup.Piece.Couleur, coup.LigneArrivee, coup.ColonneArrivee),
                _ => new Reine(coup.Piece.Couleur, coup.LigneArrivee, coup.ColonneArrivee)
            };

            nouvellePiece.ADejaBougee = true;
            Cases[coup.LigneArrivee, coup.ColonneArrivee].PlacerPiece(nouvellePiece);
        }

        public void Vider()
        {
            for (int lig = 0; lig < 8; lig++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Cases[lig, col].Piece = null;
                }
            }
        }


        #endregion

        #region Vérifications d'échec

        /// <summary>
        /// Vérifie si un roi est en échec
        /// </summary>
        public bool EstEnEchec(CouleurPiece couleurRoi)
        {
            Roi? roi = TrouverRoi(couleurRoi);
            if (roi == null)
                return false;

            return EstCaseAttaquee(roi.Ligne, roi.Colonne, couleurRoi);
        }

        /// <summary>
        /// Vérifie si une case est attaquée par une pièce adverse
        /// </summary>
        public bool EstCaseAttaquee(int ligne, int colonne, CouleurPiece couleurDefenseur)
        {
            CouleurPiece couleurAttaquant = couleurDefenseur == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            List<Piece> piecesAttaquantes = ObtenirPieces(couleurAttaquant);

            foreach (Piece piece in piecesAttaquantes)
            {
                if (piece.EstCoupValide(ligne, colonne, this))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Vérifie si un coup mettrait son propre roi en échec
        /// </summary>
        public bool CoupMettroitRoiEnEchec(Coup coup)
        {
            if (coup.Piece == null)
                return false;

            // Créer une copie de l'échiquier
            Echiquier copie = Cloner();

            // Exécuter le coup sur la copie
            copie.ExecuterCoup(coup);

            // Vérifier si le roi est en échec
            return copie.EstEnEchec(coup.Piece.Couleur);
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Vérifie si une position est valide sur l'échiquier
        /// </summary>
        public bool EstPositionValide(int ligne, int colonne)
        {
            return ligne >= 0 && ligne < 8 && colonne >= 0 && colonne < 8;
        }

        /// <summary>
        /// Clone l'échiquier
        /// </summary>
        public Echiquier Cloner()
        {
            Echiquier copie = new Echiquier(EtatPartie);

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? piece = Cases[ligne, colonne].Piece;
                    if (piece != null)
                    {
                        Piece pieceClonee = piece.Cloner();
                        copie.PlacerPiece(pieceClonee, ligne, colonne);

                        // Mettre à jour les références aux rois
                        if (pieceClonee is Roi roi)
                        {
                            if (roi.Couleur == CouleurPiece.Blanc)
                                copie.RoiBlanc = roi;
                            else
                                copie.RoiNoir = roi;
                        }
                    }
                }
            }

            return copie;
        }

        /// <summary>
        /// Convertit l'échiquier en notation FEN (Forsyth-Edwards Notation)
        /// </summary>
        public string VersNotationFEN()
        {
            StringBuilder fen = new StringBuilder();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                int casesVides = 0;

                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? piece = Cases[ligne, colonne].Piece;

                    if (piece == null)
                    {
                        casesVides++;
                    }
                    else
                    {
                        if (casesVides > 0)
                        {
                            fen.Append(casesVides);
                            casesVides = 0;
                        }

                        char symbole = piece.Type switch
                        {
                            TypePiece.Roi => 'k',
                            TypePiece.Reine => 'q',
                            TypePiece.Tour => 'r',
                            TypePiece.Fou => 'b',
                            TypePiece.Cavalier => 'n',
                            TypePiece.Pion => 'p',
                            _ => '?'
                        };

                        if (piece.Couleur == CouleurPiece.Blanc)
                            symbole = char.ToUpper(symbole);

                        fen.Append(symbole);
                    }
                }

                if (casesVides > 0)
                    fen.Append(casesVides);

                if (ligne < 7)
                    fen.Append('/');
            }

            return fen.ToString();
        }

        #endregion

        #region Override

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  a b c d e f g h");

            for (int ligne = 0; ligne < 8; ligne++)
            {
                sb.Append($"{8 - ligne} ");

                for (int colonne = 0; colonne < 8; colonne++)
                {
                    Piece? piece = Cases[ligne, colonne].Piece;
                    if (piece == null)
                    {
                        sb.Append(". ");
                    }
                    else
                    {
                        char symbole = piece.Type switch
                        {
                            TypePiece.Roi => 'K',
                            TypePiece.Reine => 'Q',
                            TypePiece.Tour => 'R',
                            TypePiece.Fou => 'B',
                            TypePiece.Cavalier => 'N',
                            TypePiece.Pion => 'P',
                            _ => '?'
                        };

                        if (piece.Couleur == CouleurPiece.Noir)
                            symbole = char.ToLower(symbole);

                        sb.Append($"{symbole} ");
                    }
                }

                sb.AppendLine($"{8 - ligne}");
            }

            sb.AppendLine("  a b c d e f g h");
            return sb.ToString();
        }

        #endregion
    }
}
