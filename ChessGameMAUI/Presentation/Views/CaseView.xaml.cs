using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChessGameMAUI.Views
{
    public partial class CaseView : ContentView
    {
        public event Action<CaseView>? CaseClicked;

        // Cache d'images (évite le clignotement dû au rechargement)
        private static readonly Dictionary<string, ImageSource> _imageCache = new();

        public CaseView()
        {
            InitializeComponent();

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => CaseClicked?.Invoke(this);
            GestureRecognizers.Add(tap);
        }

        // Couleur de fond de la case - Style bois
        public void SetBackground(bool isDark)
        {
            RunOnUi(() =>
            {
                FondCase.BackgroundColor = isDark
                    ? Color.FromArgb("#B58863")  // Brun bois foncé
                    : Color.FromArgb("#F0D9B5"); // Beige bois clair
            });
        }

        // Affiche ou efface une pièce
        public void SetPiece(string? imageName)
        {
            RunOnUi(() =>
            {
                if (string.IsNullOrEmpty(imageName))
                {
                    ImagePiece.IsVisible = false;
                    ImagePiece.Source = null;
                    return;
                }

                ImagePiece.IsAnimationPlaying = false;

                if (!_imageCache.TryGetValue(imageName, out var imgSrc))
                {
                    var baseName = Path.GetFileNameWithoutExtension(imageName);
                    var appDir = AppContext.BaseDirectory;
                    var scaledPath = Path.Combine(appDir, $"{baseName}.scale-100.png");
                    var rawPath = Path.Combine(appDir, imageName);

                    if (File.Exists(scaledPath))
                        imgSrc = ImageSource.FromFile(scaledPath);
                    else if (File.Exists(rawPath))
                        imgSrc = ImageSource.FromFile(rawPath);
                    else
                        imgSrc = ImageSource.FromFile(imageName); // fallback MAUI logical name

                    _imageCache[imageName] = imgSrc;
                }

                ImagePiece.Source = imgSrc;
                ImagePiece.IsVisible = true;
            });
        }

        // Retire les bordures de sélection
        public void ResetBorder()
        {
            RunOnUi(() =>
            {
                FondCase.Stroke = Colors.Transparent;
                FondCase.StrokeThickness = 0;
                _estEchec = false;
            });
        }

        // Met en surbrillance une case - Design moderne
        public void Highlight(bool active)
        {
            RunOnUi(() =>
            {
                _estSelectionnee = active;

                if (active)
                {
                    FondCase.Stroke = Color.FromArgb("#FFD700"); // Or moderne
                    FondCase.StrokeThickness = 4;
                }
                else
                {
                    if (_estCoupPossible)
                    {
                        FondCase.Stroke = Color.FromArgb("#4CAF50");
                        FondCase.StrokeThickness = 2;
                    }
                    else
                    {
                        ResetBorder();
                    }
                }
            });
        }

        private bool _estCoupPossible = false;
        private bool _estSelectionnee = false;
        private bool _estEchec = false;

        // Marque une case comme destination possible
        public void MarquerCoupPossible(bool active)
        {
            RunOnUi(() =>
            {
                _estCoupPossible = active;

                if (active)
                {
                    if (!_estSelectionnee)
                    {
                        FondCase.Stroke = Color.FromArgb("#4CAF50");
                        FondCase.StrokeThickness = 2;
                    }
                }
                else
                {
                    if (!_estSelectionnee)
                    {
                        ResetBorder();
                    }
                }
            });
        }

        // Met en évidence le roi en échec
        public void MarquerEchec(bool active)
        {
            RunOnUi(() =>
            {
                _estEchec = active;
                if (active)
                {
                    FondCase.Stroke = Colors.Red;
                    FondCase.StrokeThickness = 4;
                }
                else if (!_estSelectionnee && !_estCoupPossible)
                {
                    ResetBorder();
                }
            });
        }

        private static void RunOnUi(Action action)
        {
            if (MainThread.IsMainThread)
            {
                action();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
        }
    }
}
