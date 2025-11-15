using ChessGame.Models.Pieces;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace ChessGame.Convertisseurs
{
    /// <summary>
    /// Convertit une pièce d'échecs en chemin d'image (SVG par défaut).
    /// </summary>
    public class PieceVersImageConvertisseur : IValueConverter
    {
        /// <summary>
        /// Dossier relatif dans lequel se trouvent les images des pièces.
        /// </summary>
        public string DossierImages { get; set; } = "Ressources/Images";

        /// <summary>
        /// Extension de fichier à utiliser (svg par défaut).
        /// </summary>
        public string Extension { get; set; } = "svg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Piece piece)
            {
                return DependencyProperty.UnsetValue;
            }

            string extension = (parameter as string)?.Trim().TrimStart('.') ?? Extension;
            string couleur = piece.Couleur.ToString().ToLowerInvariant();
            string type = piece.Type.ToString().ToLowerInvariant();

            string relativePath = Path.Combine(DossierImages, $"{type}_{couleur}.{extension}").Replace("\\", "/");
            string packUri = $"pack://application:,,,/{relativePath}";

            try
            {
                var uri = new Uri(packUri, UriKind.Absolute);

                if (Application.GetResourceStream(uri) != null)
                {
                    return uri;
                }

                string physicalPath = Path.GetFullPath(relativePath);
                if (File.Exists(physicalPath))
                {
                    return new Uri(physicalPath, UriKind.Absolute);
                }

                return uri;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("La conversion inverse n'est pas supportée pour les images de pièces d'échecs.");
        }
    }
}
