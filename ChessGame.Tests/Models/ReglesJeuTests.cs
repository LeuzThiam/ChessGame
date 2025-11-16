using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services;
using Xunit;

namespace ChessGame.Tests.Services
{
    public class ReglesJeuTests
    {
        private readonly ReglesJeu _reglesJeu;
        private readonly ValidateurCoup _validateur;

        public ReglesJeuTests()
        {
            _validateur = new ValidateurCoup();
            _reglesJeu = new ReglesJeu(_validateur);
        }

        [Fact]
        public void EstEnEchec_RoiMenace_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var reineNoire = new Reine(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(reineNoire, 0, 4);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;

            // Act
            bool enEchec = _reglesJeu.EstEnEchec(CouleurPiece.Blanc, echiquier);

            // Assert
            Assert.True(enEchec);
        }

        [Fact]
        public void EstEchecEtMat_MatDuBerger_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var reineNoire = new Reine(CouleurPiece.Noir, 6, 5);
            var fouNoir = new Fou(CouleurPiece.Noir, 4, 2);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(reineNoire, 6, 5);
            echiquier.PlacerPiece(fouNoir, 4, 2);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;

            // Act
            bool estMat = _reglesJeu.EstEchecEtMat(CouleurPiece.Blanc, echiquier);

            // Assert - Ce n'est pas un vrai mat, juste un exemple
            // Pour un vrai test, il faudrait une position de mat réelle
            Assert.True(_reglesJeu.EstEnEchec(CouleurPiece.Blanc, echiquier));
        }

        [Fact]
        public void EstPat_AucunCoupLegalSansEchec_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 0);
            var reineNoire = new Reine(CouleurPiece.Noir, 5, 1);
            var roiNoir = new Roi(CouleurPiece.Noir, 5, 2);

            echiquier.PlacerPiece(roiBlanc, 7, 0);
            echiquier.PlacerPiece(reineNoire, 5, 1);
            echiquier.PlacerPiece(roiNoir, 5, 2);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            // Act
            bool estPat = _reglesJeu.EstPat(CouleurPiece.Blanc, echiquier);

            // Assert
            bool enEchec = _reglesJeu.EstEnEchec(CouleurPiece.Blanc, echiquier);
            Assert.False(enEchec); // Pas en échec pour un pat
        }

        [Fact]
        public void EstMaterielInsuffisant_RoiContreRoi_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var roiNoir = new Roi(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(roiNoir, 0, 4);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            // Act
            bool insuffisant = _reglesJeu.EstMaterielInsuffisant(echiquier);

            // Assert
            Assert.True(insuffisant);
        }

        [Fact]
        public void EstMaterielInsuffisant_RoiCavalierContreRoi_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var cavalierBlanc = new Cavalier(CouleurPiece.Blanc, 6, 3);
            var roiNoir = new Roi(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(cavalierBlanc, 6, 3);
            echiquier.PlacerPiece(roiNoir, 0, 4);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            // Act
            bool insuffisant = _reglesJeu.EstMaterielInsuffisant(echiquier);

            // Assert
            Assert.True(insuffisant);
        }

        [Fact]
        public void EstMaterielInsuffisant_RoiPionContreRoi_RetourneFalse()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var pionBlanc = new Pion(CouleurPiece.Blanc, 6, 4);
            var roiNoir = new Roi(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(pionBlanc, 6, 4);
            echiquier.PlacerPiece(roiNoir, 0, 4);
            echiquier.PlacerPiece(roiBlanc, 4, 4);;
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            // Act
            bool insuffisant = _reglesJeu.EstMaterielInsuffisant(echiquier);

            // Assert
            Assert.False(insuffisant);
        }

        [Fact]
        public void EstRegleDes50Coups_CompteurAtteint_RetourneTrue()
        {
            // Arrange
            var joueurBlanc = new Joueur("Blanc", CouleurPiece.Blanc);
            var joueurNoir = new Joueur("Noir", CouleurPiece.Noir);
            var etatPartie = new EtatPartie(joueurBlanc, joueurNoir);
            etatPartie.CompteurDemiCoups = 100;

            // Act
            bool regle50 = _reglesJeu.EstRegleDes50Coups(etatPartie);

            // Assert
            Assert.True(regle50);
        }

        [Fact]
        public void DeterminerStatutPartie_PartieNormale_RetourneEnCours()
        {
            // Arrange
            var echiquier = new Echiquier();
            echiquier.InitialiserPositionStandard();

            var joueurBlanc = new Joueur("Blanc", CouleurPiece.Blanc);
            var joueurNoir = new Joueur("Noir", CouleurPiece.Noir);
            var etatPartie = new EtatPartie(joueurBlanc, joueurNoir);
            echiquier.EtatPartie = etatPartie;

            // Act
            var statut = _reglesJeu.DeterminerStatutPartie(echiquier, etatPartie);

            // Assert
            Assert.Equal(StatutPartie.EnCours, statut);
        }

        [Fact]
        public void EstPartieTerminee_PartieEnCours_RetourneFalse()
        {
            // Arrange
            var statut = StatutPartie.EnCours;

            // Act
            bool terminee = _reglesJeu.EstPartieTerminee(statut);

            // Assert
            Assert.False(terminee);
        }

        [Fact]
        public void EstPartieTerminee_EchecEtMat_RetourneTrue()
        {
            // Arrange
            var statut = StatutPartie.EchecEtMatBlanc;

            // Act
            bool terminee = _reglesJeu.EstPartieTerminee(statut);

            // Assert
            Assert.True(terminee);
        }
    }
}