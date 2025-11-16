using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services.Interfaces;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChessGame.Views
{
    public partial class EchiquierView : UserControl
    {
        // ==========================
        //  CHAMPS PRIVES
        // ==========================

        private IServicePartie _service;
        private CaseView? _caseSelectionnee;
        private Piece? _pieceSelectionnee;

        public EchiquierView()
        {
            InitializeComponent();
        }

        // ==========================
        //  INITIALISATION AVEC SERVICE
        // ==========================
        public void Initialiser(IServicePartie service)
        {
            _service = service;

            // Construction graphique du plateau
            GenererEchiquier();

            // Affichage initial des pieces
            RafraichirAffichage();

            // On se met a jour quand le service joue / annule un coup
            _service.CoupJoue += (s, e) => RafraichirAffichage();
            _service.CoupAnnule += (s, e) => RafraichirAffichage();
            _service.StatutPartieChange += (s, e) => RafraichirAffichage();
        }

        // ==========================
        //  CONSTRUCTION DE LA GRILLE
        // ==========================

        private void GenererEchiquier()
        {
            GrilleEchiquier.Children.Clear();

            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var caseView = new CaseView();

                    bool isDark = (ligne + col) % 2 == 1;
                    caseView.SetBackground(isDark);

                    caseView.CaseClicked += CaseClicked;

                    Grid.SetRow(caseView, ligne);
                    Grid.SetColumn(caseView, col);

                    GrilleEchiquier.Children.Add(caseView);
                }
            }
        }

        // ==========================
        //  RAFRAICHIR L AFFICHAGE
        // ==========================

        private void RafraichirAffichage()
        {
            if (_service == null || _service.EtatPartie == null || _service.Echiquier == null)
                return;

            foreach (CaseView caseView in GrilleEchiquier.Children.OfType<CaseView>())
            {
                int ligne = Grid.GetRow(caseView);
                int col = Grid.GetColumn(caseView);

                Piece? piece = _service.ObtenirPiece(ligne, col);

                if (piece == null)
                {
                    caseView.SetPiece(null);
                }
                else
                {
                    string nomImage = ObtenirNomImage(piece);
                    caseView.SetPiece(nomImage);
                }

                // Reset surbrillance
                caseView.FondCase.BorderBrush = Brushes.Black;
                caseView.FondCase.BorderThickness = new Thickness(0.5);
            }
        }

        private string ObtenirNomImage(Piece piece)
        {
            string suffixe = piece.Couleur == CouleurPiece.Blanc ? "Blanc" : "Noir";

            return piece.Type switch
            {
                TypePiece.Pion => $"Pion{suffixe}.png",
                TypePiece.Tour => $"Tour{suffixe}.png",
                TypePiece.Cavalier => $"Cavalier{suffixe}.png",
                TypePiece.Fou => $"Fou{suffixe}.png",
                TypePiece.Reine => $"Reine{suffixe}.png",
                TypePiece.Roi => $"Roi{suffixe}.png",
                _ => ""
            };
        }

        // ==========================
        //  GESTION DES CLICS
        // ==========================

        private void CaseClicked(CaseView caseCliquee)
        {
            if (_service == null || _service.EtatPartie == null)
                return;

            int ligneCliquee = Grid.GetRow(caseCliquee);
            int colCliquee = Grid.GetColumn(caseCliquee);

            Piece? pieceSurCase = _service.ObtenirPiece(ligneCliquee, colCliquee);

            // --- A) Selection d une piece ---
            if (_pieceSelectionnee == null)
            {
                if (pieceSurCase == null)
                    return;

                if (!PeutJouerCettePiece(pieceSurCase))
                    return;

                _pieceSelectionnee = pieceSurCase;
                _caseSelectionnee = caseCliquee;

                _caseSelectionnee.FondCase.BorderBrush = Brushes.Yellow;
                _caseSelectionnee.FondCase.BorderThickness = new Thickness(3);

                return;
            }

            // --- B) Changer de piece selectionnee (meme couleur) ---
            if (pieceSurCase != null && pieceSurCase.Couleur == _pieceSelectionnee.Couleur)
            {
                _caseSelectionnee!.FondCase.BorderBrush = Brushes.Black;
                _caseSelectionnee.FondCase.BorderThickness = new Thickness(0.5);

                _pieceSelectionnee = pieceSurCase;
                _caseSelectionnee = caseCliquee;

                _caseSelectionnee.FondCase.BorderBrush = Brushes.Yellow;
                _caseSelectionnee.FondCase.BorderThickness = new Thickness(3);

                return;
            }

            // --- C) Tentative de coup ---
            TenterDeJouerCoup(ligneCliquee, colCliquee);
        }

        private bool PeutJouerCettePiece(Piece piece)
        {
            var joueurActif = _service.ObtenirJoueurActif();
            if (joueurActif == null)
                return true;

            return joueurActif.Couleur == piece.Couleur;
        }

        // ==========================
        //  LOGIQUE DE DEPLACEMENT
        // ==========================

        private void TenterDeJouerCoup(int ligneArrivee, int colArrivee)
        {
            if (_pieceSelectionnee == null || _caseSelectionnee == null)
                return;

            int ligneDepart = _pieceSelectionnee.Ligne;
            int colDepart = _pieceSelectionnee.Colonne;

            // On delegue toute la logique au service
            bool ok = _service.JouerCoup(
                ligneDepart,
                colDepart,
                ligneArrivee,
                colArrivee
            );

            // On remet l affichage de la case
            _caseSelectionnee.FondCase.BorderBrush = Brushes.Black;
            _caseSelectionnee.FondCase.BorderThickness = new Thickness(0.5);

            _pieceSelectionnee = null;
            _caseSelectionnee = null;

            if (ok)
            {
                RafraichirAffichage();
            }
        }
    }
}
