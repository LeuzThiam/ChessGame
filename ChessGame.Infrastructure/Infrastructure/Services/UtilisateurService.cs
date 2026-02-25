using ChessGame.Infrastructure.Persistence.Data;
using ChessGame.Infrastructure.Persistence.Entities;
using ChessGame.Core.Application.Interfaces;
using ChessGame.Core.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessGame.Infrastructure.Services
{
    public class UtilisateurService : IUtilisateurService
    {
        private readonly ChessDbContext _context;
        private readonly IUtilisateurSession _session;

        public UtilisateurService(ChessDbContext context, IUtilisateurSession session)
        {
            _context = context;
            _session = session;
        }

        public IUtilisateurSession Session => _session;

        public async Task<IReadOnlyList<UtilisateurInfo>> ObtenirTousAsync()
        {
            var list = await _context.Utilisateurs
                .OrderBy(u => u.Nom)
                .Select(u => new UtilisateurInfo
                {
                    Id = u.Id,
                    Nom = u.Nom,
                    Email = u.Email,
                    DateCreation = u.DateCreation
                })
                .ToListAsync();

            return list;
        }

        public async Task<UtilisateurInfo> CreerOuRecupererAsync(string nom, string? email = null)
        {
            if (string.IsNullOrWhiteSpace(nom))
                nom = "Utilisateur";

            var existing = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Nom.ToLower() == nom.ToLower());

            if (existing != null)
                return Map(existing);

            var entity = new UtilisateurEntity
            {
                Id = Guid.NewGuid(),
                Nom = nom,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                DateCreation = DateTime.UtcNow
            };

            _context.Utilisateurs.Add(entity);
            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task BasculerUtilisateurAsync(Guid id)
        {
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new InvalidOperationException("Utilisateur introuvable.");

            _session.SetUser(user.Id, user.Nom);
        }

        public void BasculerInvite(string? nom = "Invité")
        {
            _session.SetGuest(nom);
        }

        private static UtilisateurInfo Map(UtilisateurEntity u) =>
            new UtilisateurInfo
            {
                Id = u.Id,
                Nom = u.Nom,
                Email = u.Email,
                DateCreation = u.DateCreation
            };
    }
}
