using ChessGame.Core.Domain.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace ChessGameMAUI.Convertisseurs
{
    /// <summary>
    /// Convertit la couleur d’une pièce vers un Brush MAUI
    /// </summary>
    public class CouleurVersPinceauConvertisseur : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not CouleurPiece couleur)
                return Colors.Gray;

            string? param = parameter?.ToString();

            return param switch
            {
                "Texte" =>
                    couleur == CouleurPiece.Blanc ? Colors.WhiteSmoke : Colors.Black,

                "Bordure" =>
                    couleur == CouleurPiece.Blanc ? Colors.LightGray : Colors.DarkGray,

                "Fond" =>
                    couleur == CouleurPiece.Blanc ? Colors.White : Colors.Black,

                "Accent" =>
                    couleur == CouleurPiece.Blanc ? Color.FromRgb(52, 152, 219) : Color.FromRgb(231, 76, 60),

                _ =>
                    couleur == CouleurPiece.Blanc ? Colors.White : Colors.Black
            };
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Convertit CouleurPiece → string ("Blanc"/"Noir")
    /// </summary>
    public class CouleurVersNomConvertisseur : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            return value is CouleurPiece c
                ? (c == CouleurPiece.Blanc ? "Blanc" : "Noir")
                : "Inconnu";
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string nom)
                return nom.Equals("Blanc", StringComparison.OrdinalIgnoreCase)
                    ? CouleurPiece.Blanc
                    : CouleurPiece.Noir;

            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Convertit CouleurPiece → Couleur inverse (Blanc ⇄ Noir)
    /// </summary>
    public class CouleurInverseConvertisseur : IValueConverter
    {
        private readonly CouleurVersPinceauConvertisseur _conv = new();

        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not CouleurPiece couleur)
                return Colors.Gray;

            var inverse = couleur == CouleurPiece.Blanc
                ? CouleurPiece.Noir
                : CouleurPiece.Blanc;

            return _conv.Convert(inverse, targetType, parameter, culture);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
