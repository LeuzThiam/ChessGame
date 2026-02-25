using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChessGameMAUI.ViewModels.Base
{
    /// <summary>
    /// Classe de base pour tous les ViewModels
    /// Implémente INotifyPropertyChanged pour le binding MVVM
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Événement déclenché lorsqu'une propriété change
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Déclenche l'événement PropertyChanged pour une propriété
        /// </summary>
        /// <param name="propertyName">Nom de la propriété (automatique avec CallerMemberName)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Met à jour une propriété et déclenche PropertyChanged si la valeur change
        /// </summary>
        /// <typeparam name="T">Type de la propriété</typeparam>
        /// <param name="field">Champ de référence</param>
        /// <param name="value">Nouvelle valeur</param>
        /// <param name="propertyName">Nom de la propriété (automatique)</param>
        /// <returns>True si la valeur a changé</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}