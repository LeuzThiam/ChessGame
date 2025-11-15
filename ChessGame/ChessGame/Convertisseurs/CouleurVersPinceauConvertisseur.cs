using ChessGame.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChessGame.Convertisseurs
{
    /// <summary>
    /// Convertit une couleur de pièce d'échecs en pinceau (Brush) WPF
    /// </summary>
    public class CouleurVersPinceauConvertisseur : IValueConverter
    {
        /// <summary>
        /// Convertit une CouleurPiece en SolidColorBrush
        /// </summary>
        /// <param name="value">CouleurPiece (Blanc ou Noir)</param>
        /// <param name="targetType">Type cible (Brush)</param>
        /// <param name="parameter">Paramètre optionnel pour personnaliser les couleurs</param>
        /// <param name="culture">Culture</param>
        /// <returns>Brush correspondant à la couleur</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CouleurPiece couleur)
            {
                // Vérifier si un paramètre personnalisé est fourni
                string param = parameter?.ToString();

                switch (param)
                {
                    case "Texte":
                        // Couleurs pour le texte (contraste élevé)
                        return couleur == CouleurPiece.Blanc
                            ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) // Blanc cassé
                            : new SolidColorBrush(Color.FromRgb(30, 30, 30));    // Noir profond

                    case "Bordure":
                        // Couleurs pour les bordures
                        return couleur == CouleurPiece.Blanc
                            ? new SolidColorBrush(Color.FromRgb(200, 200, 200)) // Gris clair
                            : new SolidColorBrush(Color.FromRgb(50, 50, 50));    // Gris foncé

                    case "Fond":
                        // Couleurs pour les fonds
                        return couleur == CouleurPiece.Blanc
                            ? new SolidColorBrush(Color.FromRgb(248, 248, 248)) // Presque blanc
                            : new SolidColorBrush(Color.FromRgb(40, 40, 40));    // Presque noir

                    case "Accent":
                        // Couleurs d'accent
                        return couleur == CouleurPiece.Blanc
                            ? new SolidColorBrush(Color.FromRgb(52, 152, 219))  // Bleu pour blanc
                            : new SolidColorBrush(Color.FromRgb(231, 76, 60));  // Rouge pour noir

                    default:
                        // Couleurs par défaut (standard)
                        return couleur == CouleurPiece.Blanc
                            ? Brushes.White
                            : Brushes.Black;
                }
            }

            // Valeur par défaut si la conversion échoue
            return Brushes.Gray;
        }

        /// <summary>
        /// Conversion inverse (non implémentée car non nécessaire)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("La conversion inverse de Brush vers CouleurPiece n'est pas supportée.");
        }
    }

    /// <summary>
    /// Convertit une CouleurPiece en nom de couleur (string)
    /// </summary>
    public class CouleurVersNomConvertisseur : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CouleurPiece couleur)
            {
                return couleur == CouleurPiece.Blanc ? "Blanc" : "Noir";
            }

            return "Inconnu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string nom)
            {
                return nom.Equals("Blanc", StringComparison.OrdinalIgnoreCase)
                    ? CouleurPiece.Blanc
                    : CouleurPiece.Noir;
            }

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convertit une CouleurPiece en couleur inversée (pour le joueur adverse)
    /// </summary>
    public class CouleurInverseConvertisseur : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CouleurPiece couleur)
            {
                var couleurInverse = couleur == CouleurPiece.Blanc
                    ? CouleurPiece.Noir
                    : CouleurPiece.Blanc;

                // Utiliser le convertisseur de couleur pour obtenir le brush
                var convertisseur = new CouleurVersPinceauConvertisseur();
                return convertisseur.Convert(couleurInverse, targetType, parameter, culture);
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}