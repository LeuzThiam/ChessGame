using System;
using System.Globalization;
using Microsoft.Maui.Controls;


namespace ChessGameMAUI.Convertisseurs
{
    /// <summary>
    /// Convertit une valeur booléenne en bool (pour IsVisible)
    /// True → true (Visible), False → false (Hidden)
    /// </summary>
    public class BoolVersVisibiliteConvertisseur : IValueConverter
    {
        /// <summary>
        /// Convertit un booléen en bool (pour IsVisible)
        /// </summary>
        /// <param name="value">Valeur booléenne</param>
        /// <param name="targetType">Type cible (bool)</param>
        /// <param name="parameter">Paramètre optionnel pour inverser (set à "Inverse")</param>
        /// <param name="culture">Culture</param>
        /// <returns>true si true, false si false</returns>
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booleen)
            {
                // Vérifier si on doit inverser la logique
                bool inverser = parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;

                return inverser ? !booleen : booleen;
            }

            return false;
        }

        /// <summary>
        /// Convertit un bool en booléen (conversion inverse)
        /// </summary>
        /// <param name="value">Valeur bool</param>
        /// <param name="targetType">Type cible (bool)</param>
        /// <param name="parameter">Paramètre optionnel</param>
        /// <param name="culture">Culture</param>
        /// <returns>Booléen correspondant</returns>
        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booleen)
            {
                bool inverser = parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
                return inverser ? !booleen : booleen;
            }

            return false;
        }
    }

    /// <summary>
    /// Variante qui inverse la logique
    /// True → false (Hidden), False → true (Visible)
    /// </summary>
    public class BoolVersVisibiliteHiddenConvertisseur : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booleen)
            {
                bool inverser = parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;

                // Par défaut, inverse la logique
                return inverser ? booleen : !booleen;
            }

            return true; // Par défaut visible
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool booleen)
            {
                bool inverser = parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
                return inverser ? booleen : !booleen;
            }

            return false;
        }
    }
}