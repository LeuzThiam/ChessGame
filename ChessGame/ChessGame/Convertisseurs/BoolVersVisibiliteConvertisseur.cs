using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChessGame.Convertisseurs
{
    /// <summary>
    /// Convertit une valeur booléenne en Visibility
    /// True → Visible, False → Collapsed
    /// </summary>
    public class BoolVersVisibiliteConvertisseur : IValueConverter
    {
        /// <summary>
        /// Convertit un booléen en Visibility
        /// </summary>
        /// <param name="value">Valeur booléenne</param>
        /// <param name="targetType">Type cible (Visibility)</param>
        /// <param name="parameter">Paramètre optionnel pour inverser (set à "Inverse")</param>
        /// <param name="culture">Culture</param>
        /// <returns>Visibility.Visible si true, Visibility.Collapsed si false</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleen)
            {
                // Vérifier si on doit inverser la logique
                bool inverser = parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);

                if (inverser)
                {
                    return booleen ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return booleen ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Convertit une Visibility en booléen (conversion inverse)
        /// </summary>
        /// <param name="value">Valeur Visibility</param>
        /// <param name="targetType">Type cible (bool)</param>
        /// <param name="parameter">Paramètre optionnel</param>
        /// <param name="culture">Culture</param>
        /// <returns>True si Visible, False sinon</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool inverser = parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);

                bool resultat = visibility == Visibility.Visible;

                return inverser ? !resultat : resultat;
            }

            return false;
        }
    }

    /// <summary>
    /// Variante qui utilise Hidden au lieu de Collapsed
    /// True → Visible, False → Hidden (garde l'espace)
    /// </summary>
    public class BoolVersVisibiliteHiddenConvertisseur : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleen)
            {
                bool inverser = parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);

                if (inverser)
                {
                    return booleen ? Visibility.Hidden : Visibility.Visible;
                }
                else
                {
                    return booleen ? Visibility.Visible : Visibility.Hidden;
                }
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool inverser = parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase);
                bool resultat = visibility == Visibility.Visible;
                return inverser ? !resultat : resultat;
            }

            return false;
        }
    }
}