using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System.Linq;
using Microsoft.Maui.ApplicationModel;

namespace ChessGameMAUI.Views
{
    public partial class EchiquierView : ContentView
    {
        private IServicePartie _service;
        private CaseView? _caseSelectionnee;
        private Piece? _pieceSelectionnee;
        private List<Coup>? _coupsPossibles;
        
        public event Action<Piece?, List<Coup>?>? PieceSelectionneeChanged;

        public EchiquierView()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            // Ajuster la taille après le chargement
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AjusterTailleEchiquier();
            });
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            AjusterTailleEchiquier();
        }

        private void AjusterTailleEchiquier()
        {
            if (Plateau == null)
                return;

            // Obtenir la taille de la fenêtre
            var window = Application.Current?.Windows?.FirstOrDefault();
            if (window == null)
                return;

            var screenWidth = window.Width;
            var screenHeight = window.Height;

            // Utiliser la taille réelle du contrôle ou celle de la fenêtre
            double largeurDisponible = Width > 0 ? Width - 20 : screenWidth - 20;
            double hauteurDisponible = Height > 0 ? Height - 20 : screenHeight - 20;

            // Sur grands écrans, réserver de l'espace pour le panneau d'infos
            if (screenWidth >= 800)
            {
                largeurDisponible = Math.Min(largeurDisponible, screenWidth - 370); // 350px pour infos + 20px padding
            }

            // Prendre le minimum pour garder un carré
            double taille = Math.Min(largeurDisponible, hauteurDisponible);

            // Limiter entre min et max selon la taille de l'écran
            double minSize = screenWidth < 800 ? 250 : 300;
            double maxSize = screenWidth < 800 ? Math.Min(500, screenWidth - 30) : Math.Min(800, screenWidth - 370);
            
            // S'assurer que la taille est valide
            if (taille < minSize)
                taille = minSize;
            if (taille > maxSize)
                taille = maxSize;

            // Appliquer la taille
            Plateau.WidthRequest = taille;
            Plateau.HeightRequest = taille;
        }

        public void Initialiser(IServicePartie service)
        {
            _service = service;

            GenererEchiquier();
            RafraichirAffichage();
            AjusterTailleEchiquier();

            _service.CoupJoue += (_, __) => RafraichirAffichage();
            _service.CoupAnnule += (_, __) => RafraichirAffichage();
            _service.StatutPartieChange += (_, __) => RafraichirAffichage();
        }

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

        private void RafraichirAffichage()
        {
            if (_service?.EtatPartie == null)
                return;

            foreach (var caseView in GrilleEchiquier.Children.OfType<CaseView>())
            {
                int uiLigne = Grid.GetRow(caseView);
                int uiCol = Grid.GetColumn(caseView);

                // Conversion UI → logique moteur (blancs en bas, noirs en haut)
                int ligne = uiLigne;
                int col = uiCol;

                Piece? piece = _service.ObtenirPiece(ligne, col);

                caseView.SetPiece(piece == null ? null : ObtenirNomImage(piece));
                caseView.ResetBorder();
                caseView.MarquerCoupPossible(false);
                caseView.MarquerEchec(false);
            }

            // Réafficher la sélection et les coups possibles si une pièce est sélectionnée
            if (_pieceSelectionnee != null && _caseSelectionnee != null)
            {
                _caseSelectionnee.Highlight(true);
                AfficherCoupsPossibles();
            }

            MarquerRoiEnEchec();
        }

        private string ObtenirNomImage(Piece piece)
        {
            string couleur = piece.Couleur == CouleurPiece.Blanc ? "blanc" : "noir";

            return piece.Type switch
            {
                TypePiece.Pion => $"pion_{couleur}.png",
                TypePiece.Tour => $"tour_{couleur}.png",
                TypePiece.Cavalier => $"cavalier_{couleur}.png",
                TypePiece.Fou => $"fou_{couleur}.png",
                TypePiece.Reine => $"reine_{couleur}.png",
                TypePiece.Roi => $"roi_{couleur}.png",
                _ => ""
            };
        }

        private void CaseClicked(CaseView caseCliquee)
        {
            if (_service?.EtatPartie == null)
                return;

            int uiLigne = Grid.GetRow(caseCliquee);
            int uiCol = Grid.GetColumn(caseCliquee);

            // Conversion UI → logique moteur (blancs en bas, noirs en haut)
            int ligneLog = uiLigne;
            int colLog = uiCol;

            Piece? piece = _service.ObtenirPiece(ligneLog, colLog);

            // A) Première sélection
            if (_pieceSelectionnee == null)
            {
                if (piece == null || !PeutJouerCettePiece(piece))
                    return;

                _pieceSelectionnee = piece;
                _caseSelectionnee = caseCliquee;

                caseCliquee.Highlight(true);
                AfficherCoupsPossibles();
                NotifierSelection();
                return;
            }

            // B) Sélection d'une autre pièce du même joueur
            if (piece != null && piece.Couleur == _pieceSelectionnee.Couleur)
            {
                EffacerCoupsPossibles();
                _caseSelectionnee!.Highlight(false);

                _pieceSelectionnee = piece;
                _caseSelectionnee = caseCliquee;

                caseCliquee.Highlight(true);
                AfficherCoupsPossibles();
                NotifierSelection();
                return;
            }

            // C) Tentative de déplacement
            JouerCoup(ligneLog, colLog);
        }

        private bool PeutJouerCettePiece(Piece piece)
        {
            var joueur = _service.ObtenirJoueurActif();
            return joueur != null && joueur.Couleur == piece.Couleur;
        }

        private void JouerCoup(int ligneArriveeLog, int colArriveeLog)
        {
            if (_pieceSelectionnee == null)
                return;

            int lDepart = _pieceSelectionnee.Ligne;
            int cDepart = _pieceSelectionnee.Colonne;

            bool ok = _service.JouerCoup(lDepart, cDepart, ligneArriveeLog, colArriveeLog);

            EffacerCoupsPossibles();
            _caseSelectionnee?.Highlight(false);
            _pieceSelectionnee = null;
            _caseSelectionnee = null;
            _coupsPossibles = null;
            NotifierSelection();

            if (ok)
                RafraichirAffichage();
        }

        private void AfficherCoupsPossibles()
        {
            if (_pieceSelectionnee == null || _service == null)
                return;

            // Obtenir les coups possibles
            _coupsPossibles = _service.ObtenirCoupsPossibles(_pieceSelectionnee.Ligne, _pieceSelectionnee.Colonne);

            // Marquer les cases de destination possibles
            foreach (var coup in _coupsPossibles)
            {
                int uiLigne = coup.LigneArrivee;
                int uiCol = coup.ColonneArrivee;

                var caseView = GrilleEchiquier.Children
                    .OfType<CaseView>()
                    .FirstOrDefault(c => Grid.GetRow(c) == uiLigne && Grid.GetColumn(c) == uiCol);

                if (caseView != null)
                {
                    caseView.MarquerCoupPossible(true);
                }
            }
        }

        private void EffacerCoupsPossibles()
        {
            foreach (var caseView in GrilleEchiquier.Children.OfType<CaseView>())
            {
                caseView.MarquerCoupPossible(false);
            }
        }

        private void MarquerRoiEnEchec()
        {
            if (_service?.EtatPartie == null)
                return;

            // Vérifier chaque couleur
            foreach (var couleur in new[] { CouleurPiece.Blanc, CouleurPiece.Noir })
            {
                if (!_service.EstEnEchec(couleur))
                    continue;

                // Trouver le roi de cette couleur
                for (int ligne = 0; ligne < 8; ligne++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        var piece = _service.ObtenirPiece(ligne, col);
                        if (piece?.Type == TypePiece.Roi && piece.Couleur == couleur)
                        {
                            var caseView = GrilleEchiquier.Children
                                .OfType<CaseView>()
                                .FirstOrDefault(c => Grid.GetRow(c) == ligne && Grid.GetColumn(c) == col);

                            caseView?.MarquerEchec(true);
                            return;
                        }
                    }
                }
            }
        }

        private void NotifierSelection()
        {
            PieceSelectionneeChanged?.Invoke(_pieceSelectionnee, _coupsPossibles);
        }
    }
}
