using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ChessGame.Models;
using ChessGame.Models.Pieces;

namespace ChessGame.Convertisseurs
{
    /// <summary>
    /// Convertit une Piece en chemin d'image WPF (PNG).
    /// </summary>
    public class PieceVersImageConvertisseur : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Piece piece)
                return null;

            if (piece.Type == TypePiece.Aucune)
                return null;

            string couleurSuffixe = piece.Couleur == CouleurPiece.Blanc ? "Blanc" : "Noir";
            string typePrefixe = piece.Type switch
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

            // Ressource embarquee: ChessGame/Ressources/Images/{Type}{Couleur}.png
            string relativePath = $"/ChessGame;component/Ressources/Images/{typePrefixe}{couleurSuffixe}.png";
            try
            {
                var uri = new Uri(relativePath, UriKind.Relative);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
