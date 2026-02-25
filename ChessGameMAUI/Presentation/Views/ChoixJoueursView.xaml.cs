using ChessGameMAUI.Services;
using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Domain.Models;
using ChessGameMAUI.Presentation.Services;
using Microsoft.Maui.Controls;
using System;
using System.Linq;

namespace ChessGameMAUI.Views
{
    public partial class ChoixJoueursView : ContentView
    {
        private const double SeuilPetitEcran = 600; // Largeur en dessous de laquelle on réduit les tailles
        private bool _vsIa = false;

        public ChoixJoueursView()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            AjusterTaille();
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            AjusterTaille();
        }

        private void AjusterTaille()
        {
            if (Width <= 0 || Height <= 0)
                return;

            var window = Application.Current?.Windows?.FirstOrDefault();
            if (window == null)
                return;

            double screenWidth = window.Width;
            double screenHeight = window.Height;
            bool isSmallScreen = screenHeight < 700 || screenWidth < SeuilPetitEcran;

            // Ajuster le padding du Grid principal
            MainGrid.Padding = isSmallScreen ? new Thickness(15) : new Thickness(20);

            // Ajuster le titre
            if (IconeTitre != null && TitreLabel != null && SousTitreLabel != null)
            {
                if (isSmallScreen)
                {
                    IconeTitre.FontSize = 32;
                    TitreLabel.FontSize = 20;
                    SousTitreLabel.FontSize = 11;
                    TitreStack.Spacing = 5;
                    TitreStack.Margin = new Thickness(0, 5, 0, 15);
                }
                else
                {
                    IconeTitre.FontSize = 40;
                    TitreLabel.FontSize = 24;
                    SousTitreLabel.FontSize = 13;
                    TitreStack.Spacing = 6;
                    TitreStack.Margin = new Thickness(0, 10, 0, 20);
                }
            }

            // Ajuster les labels des joueurs
            if (LabelJoueurBlanc != null)
            {
                double labelFontSize = isSmallScreen ? 12 : 13;
                LabelJoueurBlanc.FontSize = labelFontSize;
                LabelJoueurNoir.FontSize = labelFontSize;
            }

            // Ajuster les champs de saisie
            if (TxtBlanc != null)
            {
                double entryHeight = isSmallScreen ? 40 : 45;
                double entryFontSize = isSmallScreen ? 14 : 15;
                double borderPadding = isSmallScreen ? 10 : 12;

                TxtBlanc.HeightRequest = entryHeight;
                TxtBlanc.FontSize = entryFontSize;
                TxtNoir.HeightRequest = entryHeight;
                TxtNoir.FontSize = entryFontSize;

                BorderJoueurBlanc.Padding = new Thickness(borderPadding, 0);
                BorderJoueurNoir.Padding = new Thickness(borderPadding, 0);
            }

            // Ajuster les boutons
            if (BtnDemarrer != null)
            {
                double btnHeight = isSmallScreen ? 45 : 50;
                double btnFontSize = isSmallScreen ? 15 : 16;
                double btnRetourHeight = isSmallScreen ? 40 : 45;
                double btnRetourFontSize = isSmallScreen ? 13 : 14;

                BtnDemarrer.HeightRequest = btnHeight;
                BtnDemarrer.FontSize = btnFontSize;
                BtnRetour.HeightRequest = btnRetourHeight;
                BtnRetour.FontSize = btnRetourFontSize;

                BoutonsStack.Spacing = isSmallScreen ? 10 : 12;
                BoutonsStack.Margin = new Thickness(0, isSmallScreen ? 10 : 15, 0, 0);
            }

            // Ajuster l'espacement général et la largeur
            ContentStack.Spacing = isSmallScreen ? 15 : 20;
            ContentStack.Padding = isSmallScreen ? new Thickness(12) : new Thickness(15);
            ContentStack.WidthRequest = isSmallScreen ? Math.Min(300, screenWidth - 40) : 350;
        }

        private async void BtnDemarrer_Click(object sender, EventArgs e)
        {
            string blanc = TxtBlanc.Text?.Trim() ?? "";
            string noir = _vsIa ? "Ordinateur" : (TxtNoir.Text?.Trim() ?? "");

            if (string.IsNullOrWhiteSpace(blanc) || string.IsNullOrWhiteSpace(noir))
            {
                Application.Current.MainPage.DisplayAlert("Erreur", "Veuillez remplir les deux noms.", "OK");
                return;
            }

            await MainPage.Instance.DemarrerPartieAsync(blanc, noir);

            if (_vsIa && MainPage.Instance.ServiceCourant != null)
            {
                int niveau = (int)Math.Round(SliderNiveau?.Value ?? 2);
                SimpleAi.Attacher(MainPage.Instance.ServiceCourant, CouleurPiece.Noir, niveau);
            }
        }

        private void BtnRetour_Click(object sender, EventArgs e)
        {
            MainPage.Instance.ChangerVue(new MainMenuView());
        }

        private void ChkVsIa_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _vsIa = e.Value;

            if (TxtNoir != null)
            {
                TxtNoir.IsEnabled = !_vsIa;
                if (_vsIa)
                {
                    TxtNoir.Text = "Ordinateur";
                }
            }
        }

        private void SliderNiveau_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (LblNiveau != null)
            {
                LblNiveau.Text = ((int)Math.Round(e.NewValue)).ToString();
            }
        }
    }
}