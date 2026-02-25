using ChessGame.Core.Application.Interfaces;
using ChessGameMAUI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;

namespace ChessGameMAUI.Views
{
    public partial class MainMenuView : ContentView
    {
        private readonly IUtilisateurSession _session;

        public MainMenuView()
        {
            InitializeComponent();
            _session = App.Services.GetRequiredService<IUtilisateurSession>();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            MettreAJourProfil();
            ConfigurerQuitter();
        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            Application.Current?.MainPage?.DisplayAlert("Information", "Gestion du profil indisponible dans cette version.", "OK");
        }

        private void MettreAJourProfil()
        {
            if (LblProfil == null)
                return;

            LblProfil.Text = _session.IsLoggedIn
                ? _session.CurrentUserName
                : $"{_session.CurrentUserName} (invité)";
        }

        private void ConfigurerQuitter()
        {
            if (BtnQuitter != null)
            {
                BtnQuitter.IsVisible = DeviceInfo.Idiom == DeviceIdiom.Desktop;
            }
        }

        private void BtnJouer_Click(object sender, EventArgs e)
        {
            MainPage.Instance.ChangerVue(new ChoixJoueursView());
        }

        private void BtnQuitter_Click(object sender, EventArgs e)
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            {
                Environment.Exit(0);
            }
        }
    }
}
