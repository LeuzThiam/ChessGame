using ChessGame.Models;
using ChessGame.Models.Pieces;
using Xunit;

namespace ChessGame.Tests.Models
{
    public class EchiquierTests
    {
        [Fact]
        public void Echiquier_InitialiserPositionStandard_PlaceToutesLesPieces()
        {
            // Arrange
            var echiquier = new Echiquier();

            // Act
            echiquier.InitialiserPositionStandard();

            // Assert
            // Vérifier les pions blancs
            for (int col = 0; col < 8; col++)
            {
                var piece = echiquier.ObtenirPiece(6, col);
                Assert.NotNull(piece);
                Assert.IsType<Pion>(piece);
                Assert.Equal(CouleurPiece.Blanc, piece.Couleur);
            }

            // Vérifier les pions noirs
            for (int col = 0; col < 8; col++)
            {
                var piece = echiquier.ObtenirPiece(1, col);
                Assert.NotNull(piece);
                Assert.IsType<Pion>(piece);
                Assert.Equal(CouleurPiece.Noir, piece.Couleur);
            }

            // Vérifier les rois
            Assert.NotNull(echiquier.RoiBlanc);
            Assert.NotNull(echiquier.RoiNoir);
            Assert.Equal(7, echiquier.RoiBlanc.Ligne);
            Assert.Equal(4, echiquier.RoiBlanc.Colonne);
        }

        [Fact]
        public void Echiquier_PlacerPiece_PlacePieceCorrectement()
        {
            // Arrange
            var echiquier = new Echiquier();
            var pion = new Pion(CouleurPiece.Blanc, 4, 4);

            // Act
            echiquier.PlacerPiece(pion, 4, 4);
            var pieceRecuperee = echiquier.ObtenirPiece(4, 4);

            // Assert
            Assert.Same(pion, pieceRecuperee);
            Assert.Equal(4, pion.Ligne);
            Assert.Equal(4, pion.Colonne);
        }

        [Fact]
        public void Echiquier_ExecuterCoup_DeplacePiece()
        {
            // Arrange
            var echiquier = new Echiquier();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            var coup = new Coup(pion, 6, 4, 4, 4);

            // Act
            bool succes = echiquier.ExecuterCoup(coup);

            // Assert
            Assert.True(succes);
            Assert.Null(echiquier.ObtenirPiece(6, 4));
            Assert.Same(pion, echiquier.ObtenirPiece(4, 4));
            Assert.Equal(4, pion.Ligne);
            Assert.True(pion.ADejaBougee);
        }

        [Fact]
        public void Echiquier_EstEnEchec_DetecteEchec()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roiBlanc = new Roi(CouleurPiece.Blanc, 4, 4);
            var reineNoire = new Reine(CouleurPiece.Noir, 4, 7);

            echiquier.PlacerPiece(roiBlanc, 4, 4);
            echiquier.PlacerPiece(reineNoire, 4, 7);
            echiquier.RoiBlanc = roiBlanc;

            // Act
            bool enEchec = echiquier.EstEnEchec(CouleurPiece.Blanc);

            // Assert
            Assert.True(enEchec);
        }

        [Fact]
        public void Echiquier_EstCaseAttaquee_DetecteMenace()
        {
            // Arrange
            var echiquier = new Echiquier();
            var tour = new Tour(CouleurPiece.Noir, 0, 0);
            echiquier.PlacerPiece(tour, 0, 0);

            // Act
            bool attaquee = echiquier.EstCaseAttaquee(0, 7, CouleurPiece.Blanc);

            // Assert
            Assert.True(attaquee);
        }

        [Fact]
        public void Echiquier_ViderEchiquier_EffaceToutesLesPieces()
        {
            // Arrange
            var echiquier = new Echiquier();
            echiquier.InitialiserPositionStandard();

            // Act
            echiquier.ViderEchiquier();

            // Assert
            for (int ligne = 0; ligne < 8; ligne++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Assert.Null(echiquier.ObtenirPiece(ligne, col));
                }
            }
            Assert.Null(echiquier.RoiBlanc);
            Assert.Null(echiquier.RoiNoir);
        }

        [Fact]
        public void Echiquier_ObtenirPieces_RetournePiecesDuneCouleur()
        {
            // Arrange
            var echiquier = new Echiquier();
            echiquier.InitialiserPositionStandard();

            // Act
            var piecesBlanches = echiquier.ObtenirPieces(CouleurPiece.Blanc);
            var piecesNoires = echiquier.ObtenirPieces(CouleurPiece.Noir);

            // Assert
            Assert.Equal(16, piecesBlanches.Count);
            Assert.Equal(16, piecesNoires.Count);
        }

        [Fact]
        public void Echiquier_VersNotationFEN_GenereNotationCorrecte()
        {
            // Arrange
            var echiquier = new Echiquier();
            echiquier.InitialiserPositionStandard();

            // Act
            string fen = echiquier.VersNotationFEN();

            // Assert
            Assert.StartsWith("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", fen);
        }

        [Fact]
        public void Echiquier_Cloner_CreeCopieIndependante()
        {
            // Arrange
            var echiquier = new Echiquier();
            var pion = new Pion(CouleurPiece.Blanc, 6, 4);
            echiquier.PlacerPiece(pion, 6, 4);

            // Act
            var clone = echiquier.Cloner();

            // Assert
            Assert.NotSame(echiquier, clone);
            var pieceClonee = clone.ObtenirPiece(6, 4);
            Assert.NotNull(pieceClonee);
            Assert.NotSame(pion, pieceClonee);
            Assert.Equal(pion.Type, pieceClonee.Type);
        }

        [Fact]
        public void Echiquier_ExecuterRoque_DeplacePiecesCorrectement()
        {
            // Arrange
            var echiquier = new Echiquier();
            var roi = new Roi(CouleurPiece.Blanc, 7, 4);
            var tour = new Tour(CouleurPiece.Blanc, 7, 7);

            echiquier.PlacerPiece(roi, 7, 4);
            echiquier.PlacerPiece(tour, 7, 7);
            echiquier.RoiBlanc = roi;

            var coup = new Coup(roi, 7, 4, 7, 6)
            {
                EstPetitRoque = true
            };

            // Act
            bool succes = echiquier.ExecuterCoup(coup);

            // Assert
            Assert.True(succes);
            Assert.Same(roi, echiquier.ObtenirPiece(7, 6));
            Assert.Same(tour, echiquier.ObtenirPiece(7, 5));
        }
    }
}