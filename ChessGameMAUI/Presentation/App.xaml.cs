using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace ChessGameMAUI
{
    public partial class App : MauiApplication
    {
        public App()
        {
            InitializeComponent();
        }

        public static new App Current => (App)MauiApplication.Current!;

        public static IServiceProvider Services =>
            Current.Handler?.MauiContext?.Services ??
            throw new InvalidOperationException("Le conteneur de services MAUI n'est pas initialisé.");

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
