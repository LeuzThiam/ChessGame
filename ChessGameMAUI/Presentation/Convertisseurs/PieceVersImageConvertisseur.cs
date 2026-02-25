using System;
using System.Globalization;
using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using Microsoft.Maui.Controls;

namespace ChessGameMAUI.Convertisseurs
{
    /// <summary>
    /// Convertit une Piece en chemin d'image MAUI (PNG).
    /// </summary>
    public class PieceVersImageConvertisseur : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is not Piece piece)
                return null;

            if (piece.Type == TypePiece.Aucune)
                return null;

            string couleurSuffixe = piece.Couleur == CouleurPiece.Blanc ? "Blanc" : "Noir";

            string? typePrefixe = piece.Type switch
            {
                TypePiece.Pion => "Pion",
                TypePiece.Tour => "Tour",
                TypePiece.Cavalier => "Cavalier",
                TypePiece.Fou => "Fou",
                TypePiece.Reine => "Reine",
                TypePiece.Roi => "Roi",
                _ => null
            };

            if (typePrefixe == null)
                return null;

            // Nom du fichier dans /Resources/Images/
            string imageName = $"{typePrefixe}{couleurSuffixe}.png";

            return ImageSource.FromFile(imageName);
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
            => throw new NotSupportedException();
    }
}
