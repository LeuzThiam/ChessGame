using ChessGameMAUI;
using ChessGameMAUI.Services;
using ChessGame.Core.Application.Interfaces;
using ChessGameMAUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace ChessGameMAUI.Views
{
    public partial class EchiquierPage : ContentPage
    {
        private readonly IServicePartie _service;
        private IServiceScope? _serviceScope;
        private const double SeuilPetitEcran = 800; // Largeur en dessous de laquelle on passe en mode vertical

        public EchiquierPage()
        {
            InitializeComponent();

            var services = App.Services;
            var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            _serviceScope = scopeFactory.CreateScope();
            _service = _serviceScope.ServiceProvider.GetRequiredService<IServicePartie>();

            // 1) Démarrer une nouvelle partie
            _service.DemarrerNouvellePartie("Blanc", "Noir", 10);

            // 2) Initialiser l'échiquier
            Echiquier.Initialiser(_service);

            // 3) Initialiser le panneau Info
            Infos.Initialiser(_service);

            // 4) Connecter l'événement de sélection de pièce
            Echiquier.PieceSelectionneeChanged += (piece, coups) =>
            {
                Infos.MettreAJourPieceSelectionnee(piece, coups);
            };

            // 5) Configurer le layout adaptatif
            MainGrid.SizeChanged += OnMainGridSizeChanged;
            ConfigurerLayout();
        }

        private void OnMainGridSizeChanged(object? sender, EventArgs e)
        {
            ConfigurerLayout();
        }

        private void ConfigurerLayout()
        {
            if (MainGrid == null || Width <= 0)
                return;

            // Si l'écran est petit, layout vertical (échiquier en haut, infos en bas)
            if (Width < SeuilPetitEcran)
            {
                MainGrid.RowDefinitions.Clear();
                MainGrid.ColumnDefinitions.Clear();
                
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetRow(EchiquierScroll, 0);
                Grid.SetColumn(EchiquierScroll, 0);
                Grid.SetRow(Infos, 1);
                Grid.SetColumn(Infos, 0);

                // Sur petits écrans, réduire la largeur du panneau d'infos
                Infos.WidthRequest = -1; // Auto
                Infos.HorizontalOptions = LayoutOptions.Fill;
            }
            else
            {
                // Layout horizontal (échiquier à gauche, infos à droite)
                MainGrid.RowDefinitions.Clear();
                MainGrid.ColumnDefinitions.Clear();
                
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                Grid.SetRow(EchiquierScroll, 0);
                Grid.SetColumn(EchiquierScroll, 0);
                Grid.SetRow(Infos, 0);
                Grid.SetColumn(Infos, 1);

                // Sur grands écrans, largeur fixe pour le panneau d'infos
                Infos.WidthRequest = 320;
                Infos.HorizontalOptions = LayoutOptions.Start;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _serviceScope?.Dispose();
            _serviceScope = null;
        }
    }
}
