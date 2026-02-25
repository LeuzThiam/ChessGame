using ChessGameMAUI.Services;
using Microsoft.Extensions.DependencyInjection;
using ChessGameMAUI.ViewModels;
using ChessGameMAUI.Views;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Application.Services;
using ChessGame.Infrastructure.Persistence;
using ChessGame.Infrastructure.Persistence.Repositories;
using ChessGame.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace ChessGameMAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ✅ DbContext en mémoire (local, pas d'API)
        builder.Services.AddDbContext<ChessDbContext>(options =>
        {
            options.UseInMemoryDatabase("ChessGameLocal");
        });

        // ✅ API HTTP CLIENT (ANDROID)
        builder.Services.AddHttpClient<IChessApiClient, ChessApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://10.0.2.2:5000/");
        });

        // ✅ Core services (scoped to match game sessions)
        builder.Services.AddScoped<IValidateurCoup, ValidateurCoup>();
        builder.Services.AddScoped<IReglesJeu, ReglesJeu>();
        builder.Services.AddScoped<IHistoriqueCoups, HistoriqueCoups>();
        builder.Services.AddScoped<ISauvegardeur, Sauvegardeur>();
        builder.Services.AddScoped<IPartieRepository, PartieRepository>();
        builder.Services.AddScoped<IServicePartie, ServicePartie>();

        // ✅ UI services
        builder.Services.AddSingleton<IUtilisateurSession, UtilisateurSession>();

        // ✅ ViewModels
        builder.Services.AddTransient<MainViewModel>();

        // ✅ Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EchiquierPage>();

        return builder.Build();
    }
}
