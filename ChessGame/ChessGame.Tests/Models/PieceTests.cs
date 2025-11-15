using ChessGame.Models;
using ChessGame.Models.Pieces;
using Xunit;

namespace ChessGame.Tests.Models
{
    public class PieceTests
    {
        private Echiquier CreerEchiquierVide()
        {
            return new Echiquier();
        }

        #region Tests du Pion

        [Fact]
        public void Pion_PeutAvancerDuneCase()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            // Act
            bool estValide = pion.EstCoupValide(5, 4, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void Pion_PeutAvancerDeuxCasesAuPremierCoup()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            // Act
            bool estValide = pion.EstCoupValide(4, 4, echiquier);

            // Assert
            Assert.True(estValide);
            Assert.False(pion.ADejaBougee);
        }

        [Fact]
        public void Pion_NePeutPasAvancerDeuxCasesApresAvoirBouge()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            pion.ADejaBougee = true;
            echiquier.PlacerPiece(pion, 6, 4);

            // Act
            bool estValide = pion.EstCoupValide(4, 4, echiquier);

            // Assert
            Assert.False(estValide);
        }

        [Fact]
        public void Pion_PeutCapturerEnDiagonale()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var pionBlanc = new Pion(CouleurPiece.Blanc, 6, 4);
            var pionNoir = new Pion(CouleurPiece.Noir, 5, 5);
            echiquier.PlacerPiece(pionBlanc, 6, 4);
            echiquier.PlacerPiece(pionNoir, 5, 5);

            // Act
            bool estValide = pionBlanc.EstCoupValide(5, 5, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void Pion_PeutEtrePromu()
        {
            // Arrange
            var pion = new Pion(CouleurPiece.Blanc, 1, 4);

            // Act & Assert
            Assert.False(pion.PeutEtrePromu());

            pion.Ligne = 0;
            Assert.True(pion.PeutEtrePromu());
        }

        #endregion

        #region Tests de la Tour

        [Fact]
        public void Tour_PeutSeDeplacerHorizontalement()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var tour = new Tour(CouleurPiece.Blanc, 7, 0);
            echiquier.PlacerPiece(tour, 7, 0);

            // Act
            bool estValide = tour.EstCoupValide(7, 7, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void Tour_PeutSeDeplacerVerticalement()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var tour = new Tour(CouleurPiece.Blanc, 7, 0);
            echiquier.PlacerPiece(tour, 7, 0);

            // Act
            bool estValide = tour.EstCoupValide(0, 0, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void Tour_NePeutPasSauterPieces()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var tour = new Tour(CouleurPiece.Blanc, 7, 0);
            var pion = new Pion(CouleurPiece.Blanc, 7, 3);
            echiquier.PlacerPiece(tour, 7, 0);
            echiquier.PlacerPiece(pion, 7, 3);

            // Act
            bool estValide = tour.EstCoupValide(7, 7, echiquier);

            // Assert
            Assert.False(estValide);
        }

        #endregion

        #region Tests du Cavalier

        [Fact]
        public void Cavalier_SeDeplacenEnL()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var cavalier = new Cavalier(CouleurPiece.Blanc, 7, 1);
            echiquier.PlacerPiece(cavalier, 7, 1);

            // Act & Assert
            Assert.True(cavalier.EstCoupValide(5, 0, echiquier)); // 2 haut, 1 gauche
            Assert.True(cavalier.EstCoupValide(5, 2, echiquier)); // 2 haut, 1 droite
            Assert.True(cavalier.EstCoupValide(6, 3, echiquier)); // 1 haut, 2 droite
        }

        [Fact]
        public void Cavalier_PeutSauterPieces()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var cavalier = new Cavalier(CouleurPiece.Blanc, 7, 1);
            var pion = new Pion(CouleurPiece.Blanc, 6, 1);
            echiquier.PlacerPiece(cavalier, 7, 1);
            echiquier.PlacerPiece(pion, 6, 1);

            // Act
            bool estValide = cavalier.EstCoupValide(5, 0, echiquier);

            // Assert
            Assert.True(estValide);
        }

        #endregion

        #region Tests du Fou

        [Fact]
        public void Fou_SeDeplacenEnDiagonale()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var fou = new Fou(CouleurPiece.Blanc, 7, 2);
            echiquier.PlacerPiece(fou, 7, 2);

            // Act & Assert
            Assert.True(fou.EstCoupValide(4, 5, echiquier));
            Assert.True(fou.EstCoupValide(0, 2, echiquier));
        }

        [Fact]
        public void Fou_NePeutPasSeDeplacerHorizontalement()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var fou = new Fou(CouleurPiece.Blanc, 7, 2);
            echiquier.PlacerPiece(fou, 7, 2);

            // Act
            bool estValide = fou.EstCoupValide(7, 5, echiquier);

            // Assert
            Assert.False(estValide);
        }

        #endregion

        #region Tests de la Reine

        [Fact]
        public void Reine_PeutSeDeplacerCommeUneTour()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var reine = new Reine(CouleurPiece.Blanc, 4, 4);
            echiquier.PlacerPiece(reine, 4, 4);

            // Act & Assert
            Assert.True(reine.EstCoupValide(4, 7, echiquier)); // Horizontal
            Assert.True(reine.EstCoupValide(0, 4, echiquier)); // Vertical
        }

        [Fact]
        public void Reine_PeutSeDeplacerCommeUnFou()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var reine = new Reine(CouleurPiece.Blanc, 4, 4);
            echiquier.PlacerPiece(reine, 4, 4);

            // Act & Assert
            Assert.True(reine.EstCoupValide(7, 7, echiquier)); // Diagonale
            Assert.True(reine.EstCoupValide(0, 0, echiquier)); // Diagonale
        }

        #endregion

        #region Tests du Roi

        [Fact]
        public void Roi_SeDeplaceDuneCase()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var roi = new Roi(CouleurPiece.Blanc, 4, 4);
            echiquier.PlacerPiece(roi, 4, 4);

            // Act & Assert
            Assert.True(roi.EstCoupValide(3, 4, echiquier)); // Haut
            Assert.True(roi.EstCoupValide(5, 5, echiquier)); // Diagonale
            Assert.True(roi.EstCoupValide(4, 5, echiquier)); // Droite
        }

        [Fact]
        public void Roi_NePeutPasSeDeplacerDePlusieursCase()
        {
            // Arrange
            var echiquier = CreerEchiquierVide();
            var roi = new Roi(CouleurPiece.Blanc, 4, 4);
            echiquier.PlacerPiece(roi, 4, 4);

            // Act
            bool estValide = roi.EstCoupValide(2, 4, echiquier);

            // Assert
            Assert.False(estValide);
        }

        #endregion

        #region Tests de Clone

        [Fact]
        public void Piece_Clone_CreeCopieIndependante()
        {
            // Arrange
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            pion.ADejaBougee = true;

            // Act
            var clone = pion.Cloner();

            // Assert
            Assert.NotSame(pion, clone);
            Assert.Equal(pion.Ligne, clone.Ligne);
            Assert.Equal(pion.Colonne, clone.Colonne);
            Assert.Equal(pion.Couleur, clone.Couleur);
            Assert.Equal(pion.ADejaBougee, clone.ADejaBougee);
        }

        #endregion
    }
}