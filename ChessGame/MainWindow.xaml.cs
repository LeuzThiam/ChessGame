using System.Windows;
using System.Windows.Controls;
using ChessGame.Views;
using ChessGame.Services;
using ChessGame.Services.Interfaces;

namespace ChessGame
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        private IServicePartie _servicePartie;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Page d’accueil au démarrage
            ChangerVue(new MainMenuView());
        }

        public void ChangerVue(UserControl nouvelleVue)
        {
            ContenuPrincipal.Children.Clear();
            ContenuPrincipal.Children.Add(nouvelleVue);
        }

        // Appelé depuis ChoixJoueursView
        public void DemarrerPartie(string joueurBlanc, string joueurNoir)
        {
            _servicePartie = new ServicePartie();
            _servicePartie.DemarrerNouvellePartie(joueurBlanc, joueurNoir, 10);

            // Créer un layout Echiquier + Info comme tu l’avais
            Grid layout = CreerLayoutEchiquier();

            ContenuPrincipal.Children.Clear();
            ContenuPrincipal.Children.Add(layout);
        }

        private Grid CreerLayoutEchiquier()
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(20)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

            // Bordure Echiquier
            Border echiquierBorder = new Border
            {
                BorderThickness = new Thickness(4),
                BorderBrush = System.Windows.Media.Brushes.SteelBlue,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Background = System.Windows.Media.Brushes.Black
            };

            EchiquierView echiquier = new EchiquierView();
            echiquier.Initialiser(_servicePartie);
            echiquierBorder.Child = echiquier;

            // Bordure Info Partie
            Border infoBorder = new Border
            {
                BorderThickness = new Thickness(4),
                BorderBrush = System.Windows.Media.Brushes.SteelBlue,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(15),
                Background = System.Windows.Media.Brushes.Black
            };

            InfoPartieView infoView = new InfoPartieView();
            infoView.Initialiser(_servicePartie);
            infoBorder.Child = infoView;

            Grid.SetColumn(echiquierBorder, 0);
            Grid.SetColumn(infoBorder, 1);

            grid.Children.Add(echiquierBorder);
            grid.Children.Add(infoBorder);

            return grid;
        }
    }
}
