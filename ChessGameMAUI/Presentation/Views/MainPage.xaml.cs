using ChessGameMAUI;
using ChessGameMAUI.Services;
using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Domain.Models;
using ChessGameMAUI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace ChessGameMAUI.Views
{
    public partial class MainPage : ContentPage
    {
        public static MainPage Instance { get; private set; }

        private readonly IChessApiClient _apiClient;
        private IServicePartie? _servicePartie;
        private IServiceScope? _serviceScope;
        private string? _remoteGameId;

        public IServicePartie? ServiceCourant => _servicePartie;

        public MainPage() : this(App.Services.GetRequiredService<IChessApiClient>())
        {
        }

        public MainPage(IChessApiClient apiClient)
        {
            InitializeComponent();
            Instance = this;
            _apiClient = apiClient;

            // Page d'accueil au démarrage
            ChangerVue(new MainMenuView());
        }

        public void ChangerVue(ContentView nouvelleVue)
        {
            ContenuPrincipal.Children.Clear();
            ContenuPrincipal.Children.Add(nouvelleVue);
        }

        // Appelé depuis ChoixJoueursView/JouerPage
        public Task DemarrerPartieAsync(string joueurBlanc, string joueurNoir, int tempsMinutes = 10)
        {
            var service = ConstruireServicePartie();
            service.DemarrerNouvellePartie(joueurBlanc, joueurNoir, tempsMinutes);
            _servicePartie = service;

            // Plus de backend distant : on désactive la sync réseau pour accélérer le démarrage.
            _remoteGameId = null;
            _servicePartie.CoupJoue -= OnCoupJoueRemote;
            _servicePartie.CoupJoue += OnCoupJoueRemote;

            AfficherLayout(service);
            return Task.CompletedTask;
        }

        public async Task ChargerPartieDepuisFichierAsync()
        {
            var service = ConstruireServicePartie();
            var fichier = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Charger une partie (PGN)",
                FileTypes = null
            });

            if (fichier == null)
                return;

            if (service.ChargerPartie(fichier.FullPath))
            {
                DemarrerPartieAvecService(service, _serviceScope!);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", "Chargement de la partie impossible.", "OK");
            }
        }

        // Appelé depuis MainMenuView pour charger une partie
        public void DemarrerPartieAvecService(IServicePartie servicePartie, IServiceScope serviceScope)
        {
            _serviceScope?.Dispose();
            _serviceScope = serviceScope;
            _servicePartie = servicePartie ?? throw new ArgumentNullException(nameof(servicePartie));

            _remoteGameId = null;
            _servicePartie.CoupJoue -= OnCoupJoueRemote;
            _servicePartie.CoupJoue += OnCoupJoueRemote;

            AfficherLayout(_servicePartie);
        }

        private Grid CreerLayoutEchiquier(IServicePartie servicePartie)
        {
            Grid grid = new Grid
            {
                Padding = new Thickness(20),
                ColumnSpacing = 20,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
                }
            };

            // Bordure Echiquier
            Border echiquierBorder = new Border
            {
                StrokeThickness = 4,
                Stroke = Colors.SteelBlue,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Padding = new Thickness(10),
                BackgroundColor = Colors.Black
            };

            EchiquierView echiquier = new EchiquierView();
            echiquier.Initialiser(servicePartie);
            echiquierBorder.Content = echiquier;

            // Bordure Info Partie
            Border infoBorder = new Border
            {
                StrokeThickness = 4,
                Stroke = Colors.SteelBlue,
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Padding = new Thickness(15),
                BackgroundColor = Colors.Black
            };

            InfoPartieView infoView = new InfoPartieView();
            infoView.Initialiser(servicePartie);
            infoBorder.Content = infoView;

            Grid.SetColumn(echiquierBorder, 0);
            Grid.SetColumn(infoBorder, 1);

            grid.Children.Add(echiquierBorder);
            grid.Children.Add(infoBorder);

            return grid;
        }

        private IServicePartie ConstruireServicePartie()
        {
            DisposeServiceScope();

            var services = App.Services;

            var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            _serviceScope = scopeFactory.CreateScope();
            _servicePartie = _serviceScope.ServiceProvider.GetRequiredService<IServicePartie>();

            return _servicePartie;
        }

        private void AfficherLayout(IServicePartie servicePartie)
        {
            Grid layout = CreerLayoutEchiquier(servicePartie);

            ContenuPrincipal.Children.Clear();
            ContenuPrincipal.Children.Add(layout);
        }

        private void DisposeServiceScope()
        {
            if (_serviceScope != null)
            {
                _serviceScope.Dispose();
                _serviceScope = null;
            }
        }

        private async Task CreerPartieDistanceAsync(string joueurBlanc, string joueurNoir)
        {
            try
            {
                var state = await _apiClient.CreerPartieAsync(joueurBlanc, joueurNoir, "Local");
                _remoteGameId = state.PartieId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Création de partie distante échouée : {ex.Message}");
            }
        }

        private async void OnCoupJoueRemote(object? sender, Coup coup)
        {
            if (string.IsNullOrWhiteSpace(_remoteGameId))
                return;

            try
            {
                await _apiClient.JouerCoupAsync(
                    _remoteGameId,
                    coup.LigneDepart,
                    coup.ColonneDepart,
                    coup.LigneArrivee,
                    coup.ColonneArrivee,
                    coup.EstPromotion ? coup.PiecePromotion.ToString() : null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Envoi coup échoué : {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            DisposeServiceScope();
            base.OnDisappearing();
        }
    }
}
