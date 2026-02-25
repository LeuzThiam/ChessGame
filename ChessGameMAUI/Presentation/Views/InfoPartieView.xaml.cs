using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGameMAUI.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGameMAUI.Views
{
    public partial class InfoPartieView : ContentView
    {
        private InfoPartieViewModel _viewModel;
        private IServicePartie? _servicePartie;
        private Piece? _pieceSelectionnee;
        private List<Coup>? _coupsPossibles;
        private bool _estEnPause;

        public InfoPartieView()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        public void Initialiser(IServicePartie service)
        {
            _servicePartie = service;
            _viewModel = new InfoPartieViewModel(service);
            BindingContext = _viewModel;
            _viewModel.ActualiserInfos();
            AjusterPaddingPourPetitEcran();

            if (service != null)
            {
                service.CoupJoue += (s, e) => MettreAJourJoueurActif();
                service.StatutPartieChange += (s, e) => MettreAJourJoueurActif();
                MettreAJourJoueurActif();
            }
        }

        public void MettreAJourPieceSelectionnee(Piece? piece, List<Coup>? coupsPossibles)
        {
            _pieceSelectionnee = piece;
            _coupsPossibles = coupsPossibles;
            ActualiserAffichagePiece();
        }

        private void ActualiserAffichagePiece()
        {
            if (PieceInfoPanel == null || PieceNomLabel == null || PieceDetailsLabel == null || CoupsPossiblesLabel == null)
                return;

            if (_pieceSelectionnee == null)
            {
                PieceInfoPanel.IsVisible = false;
                return;
            }

            PieceInfoPanel.IsVisible = true;

            string nomPiece = ObtenirNomPiece(_pieceSelectionnee.Type);
            string couleur = _pieceSelectionnee.Couleur == CouleurPiece.Blanc ? "Blanc" : "Noir";
            PieceNomLabel.Text = $"{nomPiece} ({couleur})";

            string colonne = ((char)('A' + _pieceSelectionnee.Colonne)).ToString();
            int ligne = 8 - _pieceSelectionnee.Ligne;
            PieceDetailsLabel.Text = $"Position : {colonne}{ligne}";

            if (_coupsPossibles != null && _coupsPossibles.Count > 0)
            {
                CoupsPossiblesLabel.Text = $"{_coupsPossibles.Count} coup(s) possible(s)";
                CoupsPossiblesLabel.TextColor = Color.FromArgb("#4CAF50");
            }
            else
            {
                CoupsPossiblesLabel.Text = "Aucun coup possible";
                CoupsPossiblesLabel.TextColor = Color.FromArgb("#e67e22");
            }
        }

        private string ObtenirNomPiece(TypePiece type)
        {
            return type switch
            {
                TypePiece.Pion => "Pion",
                TypePiece.Tour => "Tour",
                TypePiece.Cavalier => "Cavalier",
                TypePiece.Fou => "Fou",
                TypePiece.Reine => "Reine",
                TypePiece.Roi => "Roi",
                _ => "Inconnu"
            };
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            AjusterPaddingPourPetitEcran();
        }

        private void AjusterPaddingPourPetitEcran()
        {
            if (Width <= 0)
                return;

            var border = this.Content as ScrollView;
            if (border?.Content is Border borderContent)
            {
                borderContent.Padding = Width < 640 ? new Thickness(12) : new Thickness(18);
            }
        }

        private void MettreAJourJoueurActif()
        {
            if (_servicePartie == null || JoueurBlancCard == null || JoueurNoirCard == null || LblTraitBlanc == null || LblTraitNoir == null)
                return;

            // Les événements du service peuvent arriver depuis un thread en arrière-plan.
            // Assurons-nous de mettre à jour le visuel sur le thread UI pour éviter les COMExceptions WinUI.
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                var joueurActif = _servicePartie.ObtenirJoueurActif();
                if (joueurActif == null)
                    return;

                JoueurBlancCard.BackgroundColor = Color.FromArgb("#182033");
                JoueurNoirCard.BackgroundColor = Color.FromArgb("#182033");
                JoueurBlancCard.Stroke = Colors.Transparent;
                JoueurNoirCard.Stroke = Colors.Transparent;
                LblTraitBlanc.IsVisible = false;
                LblTraitNoir.IsVisible = false;

                if (joueurActif.Couleur == CouleurPiece.Blanc)
                {
                    JoueurBlancCard.BackgroundColor = Color.FromArgb("#1d2b44");
                    JoueurBlancCard.Stroke = Color.FromArgb("#27ae60");
                    JoueurBlancCard.StrokeThickness = 2;
                    LblTraitBlanc.IsVisible = true;
                }
                else
                {
                    JoueurNoirCard.BackgroundColor = Color.FromArgb("#1d2b44");
                    JoueurNoirCard.Stroke = Color.FromArgb("#e67e22");
                    JoueurNoirCard.StrokeThickness = 2;
                    LblTraitNoir.IsVisible = true;
                }
            });
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (_servicePartie == null)
                return;

            var bouton = sender as Button;

            if (_estEnPause)
            {
                _servicePartie.ReprendrePartie();
                bouton!.Text = "Pause";
                _estEnPause = false;
            }
            else
            {
                _servicePartie.MettrePause();
                bouton!.Text = "Reprendre";
                _estEnPause = true;
            }
        }

        public InfoPartieViewModel GetViewModel() => _viewModel;
    }
}
