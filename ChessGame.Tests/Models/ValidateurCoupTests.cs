using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services;
using Xunit;

namespace ChessGame.Tests.Services
{
    public class ValidateurCoupTests
    {
        private readonly ValidateurCoup _validateur;

        public ValidateurCoupTests()
        {
            _validateur = new ValidateurCoup();
        }

        [Fact]
        public void EstCoupLegal_CoupValide_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            var coup = new Coup(pion, 6, 4, 5, 4);

            // Act
            bool estLegal = _validateur.EstCoupLegal(coup, echiquier);

            // Assert
            Assert.True(estLegal);
        }

        [Fact]
        public void EstCoupLegal_CoupMettantRoiEnEchec_RetourneFalse()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var tourBlanche = new Tour(CouleurPiece.Blanc, 7, 3);
            var reineNoire = new Reine(CouleurPiece.Noir, 0, 3);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(tourBlanche, 7, 3);
            echiquier.PlacerPiece(reineNoire, 0, 3);
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            // La tour protège le roi, la bouger met en échec
            var coup = new Coup(tourBlanche, 7, 3, 7, 2);

            // Act
            bool estLegal = _validateur.EstCoupLegal(coup, echiquier);

            // Assert
            Assert.False(estLegal);
        }

        [Fact]
        public void ObtenirCoupsLegaux_RetourneSeulementCoupsLegaux()
        {
            // Arrange
            var echiquier = new Echiquier();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            // Act
            var coupsLegaux = _validateur.ObtenirCoupsLegaux(pion, echiquier);

            // Assert
            Assert.NotEmpty(coupsLegaux);
            Assert.All(coupsLegaux, coup =>
                Assert.True(_validateur.EstCoupLegal(coup, echiquier))
            );
        }

        [Fact]
        public void ValiderRoque_RoqueValide_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roi = new Roi(CouleurPiece.Blanc, 7, 4);
            var tour = new Tour(CouleurPiece.Blanc, 7, 7);

            echiquier.PlacerPiece(roi, 7, 4);
            echiquier.PlacerPiece(tour, 7, 7);
            echiquier.PlacerPiece(roi, 4, 4);

            var coup = new Coup(roi, 7, 4, 7, 6)
            {
                EstPetitRoque = true
            };

            // Act
            bool estValide = _validateur.ValiderRoque(coup, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void ValiderRoque_RoiABouge_RetourneFalse()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roi = new Roi(CouleurPiece.Blanc, 7, 4);
            roi.ADejaBougee = true;
            var tour = new Tour(CouleurPiece.Blanc, 7, 7);

            echiquier.PlacerPiece(roi, 7, 4);
            echiquier.PlacerPiece(tour, 7, 7);
            echiquier.PlacerPiece(roi, 4, 4);

            var coup = new Coup(roi, 7, 4, 7, 6)
            {
                EstPetitRoque = true
            };

            // Act
            bool estValide = _validateur.ValiderRoque(coup, echiquier);

            // Assert
            Assert.False(estValide);
        }

        [Fact]
        public void ValiderRoque_RoiEnEchec_RetourneFalse()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roi = new Roi(CouleurPiece.Blanc, 7, 4);
            var tour = new Tour(CouleurPiece.Blanc, 7, 7);
            var reineNoire = new Reine(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roi, 7, 4);
            echiquier.PlacerPiece(tour, 7, 7);
            echiquier.PlacerPiece(reineNoire, 0, 4);
            echiquier.PlacerPiece(roi, 4, 4);

            var coup = new Coup(roi, 7, 4, 7, 6)
            {
                EstPetitRoque = true
            };

            // Act
            bool estValide = _validateur.ValiderRoque(coup, echiquier);

            // Assert
            Assert.False(estValide);
        }

        [Fact]
        public void ObtenirTousCoupsLegaux_RetourneTousLesCoupsDisponibles()
        {
            // Arrange
            var echiquier = new Echiquier();
            echiquier.InitialiserPositionStandard();

            // Act
            var coupsLegaux = _validateur.ObtenirTousCoupsLegaux(CouleurPiece.Blanc, echiquier);

            // Assert
            Assert.NotEmpty(coupsLegaux);
            // Position initiale: 20 coups possibles pour les blancs
            Assert.Equal(20, coupsLegaux.Count);
        }

        [Fact]
        public void CoupMetRoiEnEchec_CoupDangereux_RetourneTrue()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var pionBlanc = new Pion(CouleurPiece.Blanc, 6, 4);
            var reineNoire = new Reine(CouleurPiece.Noir, 0, 4);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(pionBlanc, 6, 4);
            echiquier.PlacerPiece(reineNoire, 0, 4);
            echiquier.PlacerPiece(roiBlanc, 4, 4);

            var coup = new Coup(pionBlanc, 6, 4, 5, 4);

            // Act
            bool metEnEchec = _validateur.CoupMetRoiEnEchec(coup, echiquier);

            // Assert
            Assert.True(metEnEchec);
        }
    }
}