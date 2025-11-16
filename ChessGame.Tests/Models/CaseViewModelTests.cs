using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.ViewModels;
using Xunit;

namespace ChessGame.Tests.ViewModels
{
    public class CaseViewModelTests
    {
        [Fact]
        public void CaseViewModel_Creation_InitialiseCorrectement()
        {
            // Arrange & Act
            var caseVM = new CaseViewModel(4, 4);

            // Assert
            Assert.Equal(4, caseVM.Ligne);
            Assert.Equal(4, caseVM.Colonne);
            Assert.Null(caseVM.Piece);
            Assert.False(caseVM.EstSelectionnee);
            Assert.False(caseVM.EstCoupPossible);
        }

        [Fact]
        public void EstClaire_CaseClaire_RetourneTrue()
        {
            // Arrange
            var caseVM = new CaseViewModel(0, 0);

            // Act
            bool estClaire = caseVM.EstClaire;

            // Assert
            Assert.True(estClaire);
        }

        [Fact]
        public void EstClaire_CaseFoncee_RetourneFalse()
        {
            // Arrange
            var caseVM = new CaseViewModel(0, 1);

            // Act
            bool estClaire = caseVM.EstClaire;

            // Assert
            Assert.False(estClaire);
        }

        [Fact]
        public void NotationAlgebrique_RetourneFormatCorrect()
        {
            // Arrange
            var caseVM = new CaseViewModel(7, 0); // a1

            // Act
            string notation = caseVM.NotationAlgebrique;

            // Assert
            Assert.Equal("a1", notation);
        }

        [Fact]
        public void NotationAlgebrique_CaseE4_RetourneE4()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4); // e4

            // Act
            string notation = caseVM.NotationAlgebrique;

            // Assert
            Assert.Equal("e4", notation);
        }

        [Fact]
        public void Piece_Modification_DeclenchePropertyChanged()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);
            bool eventDeclenche = false;
            caseVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(caseVM.Piece))
                    eventDeclenche = true;
            };

            // Act
            caseVM.Piece = new Pion(CouleurPiece.Blanc, 4, 4);

            // Assert
            Assert.True(eventDeclenche);
        }

        [Fact]
        public void EstSelectionnee_Modification_MiseAJourCouleurFond()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);
            bool couleurFondChanged = false;
            caseVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(caseVM.CouleurFond))
                    couleurFondChanged = true;
            };

            // Act
            caseVM.EstSelectionnee = true;

            // Assert
            Assert.True(caseVM.EstSelectionnee);
            Assert.True(couleurFondChanged);
        }

        [Fact]
        public void ReinitialiserEtat_ResetTousLesEtats()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);
            caseVM.EstSelectionnee = true;
            caseVM.EstCoupPossible = true;
            caseVM.EstDernierCoup = true;
            caseVM.EstEnEchec = true;

            // Act
            caseVM.ReinitialiserEtat();

            // Assert
            Assert.False(caseVM.EstSelectionnee);
            Assert.False(caseVM.EstCoupPossible);
            Assert.False(caseVM.EstDernierCoup);
            Assert.False(caseVM.EstEnEchec);
        }

        [Fact]
        public void AfficherIndicateurCoup_EstCoupPossible_RetourneTrue()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);
            caseVM.EstCoupPossible = true;

            // Act
            bool afficher = caseVM.AfficherIndicateurCoup;

            // Assert
            Assert.True(afficher);
        }

        [Fact]
        public void ToString_AvecPiece_RetourneDescriptionComplete()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);
            caseVM.Piece = new Pion(CouleurPiece.Blanc, 4, 4);

            // Act
            string description = caseVM.ToString();

            // Assert
            Assert.Contains("e4", description);
            Assert.DoesNotContain("Vide", description);
        }

        [Fact]
        public void ToString_SansPiece_RetourneVide()
        {
            // Arrange
            var caseVM = new CaseViewModel(4, 4);

            // Act
            string description = caseVM.ToString();

            // Assert
            Assert.Contains("Vide", description);
        }
    }
}