using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessGame.Views
{
    public partial class CaseView : UserControl
    {
        // EVENT envoyé à EchiquierView quand la case est cliquée
        public event Action<CaseView>? CaseClicked;

        public CaseView()
        {
            InitializeComponent();
        }

        // Gestion du clic sur la case
        private void OnCaseClick(object sender, MouseButtonEventArgs e)
        {
            CaseClicked?.Invoke(this);
        }

        // Appliquer la couleur (noir/blanc)
        public void SetBackground(bool isDark)
        {
            FondCase.Background = isDark
                ? new SolidColorBrush(Color.FromRgb(118, 150, 86))   // vert foncé
                : new SolidColorBrush(Color.FromRgb(238, 238, 210)); // beige clair
        }

        // Afficher une pièce
        public void SetPiece(string? imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                PieceImage.Source = null;
                PieceImage.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var uri = new Uri($"pack://application:,,,/Ressources/Images/{imageFileName}");
                PieceImage.Source = new BitmapImage(uri);
                PieceImage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement image : " + ex.Message);
            }
        }
    }
}
