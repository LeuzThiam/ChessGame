using ChessGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChessGame.Infrastructure.Persistence.Data
{
    public class ChessDbContext : DbContext
    {
        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options)
        {
        }

        public DbSet<PartieEntity> Parties => Set<PartieEntity>();
        public DbSet<CoupEntity> Coups => Set<CoupEntity>();
        public DbSet<UtilisateurEntity> Utilisateurs => Set<UtilisateurEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PartieEntity>(entity =>
            {
                entity.ToTable("Parties");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.JoueurBlancNom).HasMaxLength(100).IsRequired();
                entity.Property(p => p.JoueurNoirNom).HasMaxLength(100).IsRequired();
                entity.Property(p => p.GagnantNom).HasMaxLength(100);
                entity.Property(p => p.PGN).HasColumnType("nvarchar(max)");
                entity.Property(p => p.FEN).HasMaxLength(128);
                entity.Property(p => p.Statut).HasConversion<int>();
                entity.Property(p => p.TypeFin).HasConversion<int>();

                entity.HasOne(p => p.JoueurBlanc)
                      .WithMany(u => u.PartiesEnBlanc)
                      .HasForeignKey(p => p.JoueurBlancId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.JoueurNoir)
                      .WithMany(u => u.PartiesEnNoir)
                      .HasForeignKey(p => p.JoueurNoirId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.Coups)
                      .WithOne(c => c.Partie)
                      .HasForeignKey(c => c.PartieId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CoupEntity>(entity =>
            {
                entity.ToTable("Coups");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Notation).HasMaxLength(64);
                entity.Property(c => c.PieceType).HasConversion<int>();
                entity.Property(c => c.PiecePromotion).HasConversion<int>();
                entity.Property(c => c.PieceCouleur).HasConversion<int>();
            });

            modelBuilder.Entity<UtilisateurEntity>(entity =>
            {
                entity.ToTable("Utilisateurs");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Nom).HasMaxLength(200).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(320);
                entity.Property(u => u.DateCreation);
            });
        }
    }
}

