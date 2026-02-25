using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// Service de gestion de l'historique des coups
    /// </summary>
    public class HistoriqueCoups : IHistoriqueCoups
    {
        private List<Coup> _coups;
        private int _indexActuel;

        #region Constructeur

        public HistoriqueCoups()
        {
            _coups = new List<Coup>();
            _indexActuel = -1;
        }

        #endregion

        #region Gestion de base

        /// <summary>
        /// Ajoute un coup à l'historique
        /// </summary>
        public void AjouterCoup(Coup coup)
        {
            if (coup == null)
                throw new ArgumentNullException(nameof(coup));

            // Si on est en train de naviguer dans l'historique, supprimer les coups après
            if (_indexActuel < _coups.Count - 1)
            {
                _coups.RemoveRange(_indexActuel + 1, _coups.Count - _indexActuel - 1);
            }

            _coups.Add(coup);
            _indexActuel = _coups.Count - 1;
        }

        /// <summary>
        /// Retire le dernier coup de l'historique
        /// </summary>
        public Coup RetirerDernierCoup()
        {
            if (_coups.Count == 0)
                return null;

            Coup dernierCoup = _coups[_coups.Count - 1];
            _coups.RemoveAt(_coups.Count - 1);
            _indexActuel = _coups.Count - 1;

            return dernierCoup;
        }

        /// <summary>
        /// Obtient le dernier coup joué
        /// </summary>
        public Coup ObtenirDernierCoup()
        {
            return _coups.Count > 0 ? _coups[_coups.Count - 1] : null;
        }

        /// <summary>
        /// Obtient tous les coups de l'historique
        /// </summary>
        public List<Coup> ObtenirTousLesCoups()
        {
            return new List<Coup>(_coups);
        }

        /// <summary>
        /// Obtient un coup à un index spécifique
        /// </summary>
        public Coup ObtenirCoup(int index)
        {
            if (index < 0 || index >= _coups.Count)
                return null;

            return _coups[index];
        }

        /// <summary>
        /// Obtient le nombre de coups dans l'historique
        /// </summary>
        public int ObtenirNombreCoups()
        {
            return _coups.Count;
        }

        #endregion

        #region Filtrage

        /// <summary>
        /// Obtient les coups d'un joueur spécifique
        /// </summary>
        public List<Coup> ObtenirCoupsParJoueur(CouleurPiece couleur)
        {
            return _coups.Where(c => c.Piece.Couleur == couleur).ToList();
        }

        #endregion

        #region Notation et formatage

        /// <summary>
        /// Obtient l'historique en notation algébrique
        /// </summary>
        public List<string> ObtenirHistoriqueNotation()
        {
            return _coups.Select(c => c.NotationAlgebrique).ToList();
        }

        /// <summary>
        /// Obtient l'historique formaté pour l'affichage
        /// </summary>
        public string ObtenirHistoriqueFormate(bool avecNumeros = true)
        {
            if (_coups.Count == 0)
                return "Aucun coup joué";

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _coups.Count; i++)
            {
                // Numéro de coup (pour les blancs)
                if (i % 2 == 0)
                {
                    if (avecNumeros)
                    {
                        sb.Append($"{(i / 2) + 1}. ");
                    }
                }

                // Ajouter le coup
                sb.Append(_coups[i].NotationAlgebrique);

                // Espace entre les coups, nouvelle ligne tous les 6 coups
                if (i < _coups.Count - 1)
                {
                    if (i % 2 == 1 && avecNumeros)
                    {
                        sb.Append("  ");

                        // Nouvelle ligne tous les 3 coups complets (6 demi-coups)
                        if ((i + 1) % 6 == 0)
                        {
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigue vers un coup spécifique dans l'historique
        /// </summary>
        public bool NaviguerVersCoup(int index)
        {
            if (index < -1 || index >= _coups.Count)
                return false;

            _indexActuel = index;
            return true;
        }

        /// <summary>
        /// Obtient l'index du coup actuel lors de la navigation
        /// </summary>
        public int ObtenirIndexActuel()
        {
            return _indexActuel;
        }

        /// <summary>
        /// Vérifie si on peut revenir en arrière dans l'historique
        /// </summary>
        public bool PeutReculer()
        {
            return _indexActuel > -1;
        }

        /// <summary>
        /// Vérifie si on peut avancer dans l'historique
        /// </summary>
        public bool PeutAvancer()
        {
            return _indexActuel < _coups.Count - 1;
        }

        /// <summary>
        /// Recule d'un coup dans l'historique
        /// </summary>
        public bool Reculer()
        {
            if (!PeutReculer())
                return false;

            _indexActuel--;
            return true;
        }

        /// <summary>
        /// Avance d'un coup dans l'historique
        /// </summary>
        public bool Avancer()
        {
            if (!PeutAvancer())
                return false;

            _indexActuel++;
            return true;
        }

        #endregion

        #region Statistiques

        /// <summary>
        /// Obtient les statistiques de l'historique
        /// </summary>
        public StatistiquesHistorique ObtenirStatistiques()
        {
            var stats = new StatistiquesHistorique
            {
                NombreTotal = _coups.Count,
                CoupsBlancs = _coups.Count(c => c.Piece.Couleur == CouleurPiece.Blanc),
                CoupsNoirs = _coups.Count(c => c.Piece.Couleur == CouleurPiece.Noir),
                NombreCaptures = _coups.Count(c => c.EstCapture()),
                NombreEchecs = _coups.Count(c => c.DonneEchec),
                NombreRoques = _coups.Count(c => c.EstPetitRoque || c.EstGrandRoque),
                NombrePromotions = _coups.Count(c => c.EstPromotion),
                NombreEnPassant = _coups.Count(c => c.EstEnPassant)
            };

            // Coup le plus long (en distance)
            if (_coups.Count > 0)
            {
                stats.CoupLePlusLong = _coups
                    .OrderByDescending(c => Math.Abs(c.LigneArrivee - c.LigneDepart) +
                                          Math.Abs(c.ColonneArrivee - c.ColonneDepart))
                    .FirstOrDefault();

                // Pièce la plus active
                var groupesPieces = _coups
                    .GroupBy(c => c.Piece.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (groupesPieces != null)
                {
                    stats.PieceLaPlusActive = groupesPieces.Key;
                }
            }

            return stats;
        }

        #endregion

        #region Utilitaires

        /// <summary>
        /// Efface tout l'historique
        /// </summary>
        public void Effacer()
        {
            _coups.Clear();
            _indexActuel = -1;
        }

        /// <summary>
        /// Clone l'historique
        /// </summary>
        public IHistoriqueCoups Cloner()
        {
            var clone = new HistoriqueCoups();
            clone._coups = new List<Coup>(_coups.Select(c => c.Cloner()));
            clone._indexActuel = _indexActuel;
            return clone;
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return $"Historique: {_coups.Count} coups, Index actuel: {_indexActuel}";
        }

        #endregion
    }
}