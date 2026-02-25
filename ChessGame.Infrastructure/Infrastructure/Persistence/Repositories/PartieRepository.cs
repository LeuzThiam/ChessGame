using ChessGame.Infrastructure.Persistence.Data;
using ChessGame.Infrastructure.Persistence.Entities;
using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ChessGame.Infrastructure.Persistence.Repositories
{
    public class PartieRepository : IPartieRepository
    {
        private readonly ChessDbContext _context;
        private readonly IUtilisateurSession? _session;

        public PartieRepository(ChessDbContext context, IUtilisateurSession? session = null)
        {
            _context = context;
            _session = session;
        }

        public void EnregistrerPartie(EtatPartie etatPartie, string pgn, string? fen)
        {
            if (etatPartie == null)
                throw new ArgumentNullException(nameof(etatPartie));

            var joueurBlancNom = etatPartie.JoueurBlanc?.Nom ?? "Joueur Blanc";
            var joueurNoirNom = etatPartie.JoueurNoir?.Nom ?? "Joueur Noir";
            var nominatifGagnant = etatPartie.Gagnant?.Nom;

            // Session optionnelle : si utilisateur connecté, on tente de lier la partie à son profil.
            var sessionUser = _session?.IsLoggedIn == true
                ? AssurerUtilisateurConnecte(_session.CurrentUserId!.Value, _session.CurrentUserName)
                : null;

            var partie = new PartieEntity
            {
                DateDebut = etatPartie.DateDebut,
                DateFin = etatPartie.DateFin,
                Statut = etatPartie.Statut,
                TypeFin = etatPartie.TypeFin,
                JoueurBlancNom = joueurBlancNom,
                JoueurNoirNom = joueurNoirNom,
                GagnantNom = nominatifGagnant,
                JoueurBlancId = sessionUser != null && joueurBlancNom.Equals(sessionUser.Nom, StringComparison.OrdinalIgnoreCase)
                    ? sessionUser.Id
                    : null,
                JoueurNoirId = sessionUser != null && joueurNoirNom.Equals(sessionUser.Nom, StringComparison.OrdinalIgnoreCase)
                    ? sessionUser.Id
                    : null,
                TempsBlancRestant = etatPartie.JoueurBlanc?.TempsRestant ?? TimeSpan.Zero,
                TempsNoirRestant = etatPartie.JoueurNoir?.TempsRestant ?? TimeSpan.Zero,
                PGN = string.IsNullOrWhiteSpace(pgn) ? null : pgn,
                FEN = string.IsNullOrWhiteSpace(fen) ? null : fen
            };

            var coups = etatPartie.HistoriqueCoups
                .Select((coup, index) =>
                {
                    var entity = MapCoup(coup, index + 1);
                    entity.Partie = partie;
                    return entity;
                })
                .ToList();

            partie.Coups = coups;
            partie.NombreCoups = coups.Count;

            _context.Parties.Add(partie);
            _context.SaveChanges();
        }

        /// <summary>
        /// Retourne l'utilisateur connecté (via session), en le créant si besoin par Id/nom.
        /// </summary>
        private UtilisateurEntity AssurerUtilisateurConnecte(Guid id, string nom)
        {
            var utilisateur = _context.Utilisateurs
                .FirstOrDefault(u => u.Id == id);

            if (utilisateur != null)
                return utilisateur;

            utilisateur = new UtilisateurEntity
            {
                Id = id,
                Nom = string.IsNullOrWhiteSpace(nom) ? "Utilisateur" : nom,
                DateCreation = DateTime.UtcNow
            };

            _context.Utilisateurs.Add(utilisateur);
            // Sauvegarde déléguée au SaveChanges final
            return utilisateur;
        }

        private static CoupEntity MapCoup(Coup coup, int ordre)
        {
            return new CoupEntity
            {
                Ordre = ordre,
                LigneDepart = coup.LigneDepart,
                ColonneDepart = coup.ColonneDepart,
                LigneArrivee = coup.LigneArrivee,
                ColonneArrivee = coup.ColonneArrivee,
                PieceType = coup.Piece?.Type ?? TypePiece.Aucune,
                PieceCouleur = coup.Piece?.Couleur ?? CouleurPiece.Blanc,
                PiecePromotion = coup.PiecePromotion,
                EstCapture = coup.EstCapture(),
                EstPromotion = coup.EstPromotion,
                EstEnPassant = coup.EstEnPassant,
                EstPetitRoque = coup.EstPetitRoque,
                EstGrandRoque = coup.EstGrandRoque,
                DonneEchec = coup.DonneEchec,
                DonneEchecEtMat = coup.DonneEchecEtMat,
                Notation = string.IsNullOrWhiteSpace(coup.NotationAlgebrique)
                    ? coup.NotationLongue
                    : coup.NotationAlgebrique
            };
        }

        public IQueryable<object> ObtenirToutes()
        {
            return _context.Parties
                .Include(p => p.Coups)
                .OrderByDescending(p => p.DateDebut)
                .Cast<object>();
        }

        public object? ObtenirParId(Guid id)
        {
            return _context.Parties
                .Include(p => p.Coups)
                .FirstOrDefault(p => p.Id == id);
        }

        public void Supprimer(Guid id)
        {
            var partie = _context.Parties.FirstOrDefault(p => p.Id == id);
            if (partie == null) return;
            _context.Parties.Remove(partie);
            _context.SaveChanges();
        }
    }
}
