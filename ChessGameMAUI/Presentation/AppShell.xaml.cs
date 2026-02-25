using ChessGameMAUI.Views;
using Microsoft.Maui.Controls;

namespace ChessGameMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(EchiquierPage), typeof(EchiquierPage));
        }
    }
}
