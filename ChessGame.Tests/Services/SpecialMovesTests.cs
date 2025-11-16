using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services;
using ChessGame.Services.Interfaces;
using Xunit;

namespace ChessGame.Tests.Services
{
    /// <summary>
    /// Tests de haut niveau pour verifier les coups speciaux
    /// (roque, prise en passant, promotion).
    /// Ces tests se basent sur l'implementation existante de l'echequier
    /// et du validateur de coups.
    /// </summary>
    public class SpecialMovesTests
    {
        private readonly ValidateurCoup _validateur = new();

        private Echiquier CreerEchiquierAvecEtat(out EtatPartie etat)
        {
            etat = new EtatPartie();
            var echiquier = new Echiquier(etat);
            return echiquier;
        }

        [Fact]
        public void PetitRoqueBlanc_EstValide_DansPositionSimple()
        {
            // Arrange
            var echiquier = CreerEchiquierAvecEtat(out var etat);
            var roiBlanc = new Roi(CouleurPiece.Blanc, 7, 4);
            var tourBlanche = new Tour(CouleurPiece.Blanc, 7, 7);

            echiquier.PlacerPiece(roiBlanc, 7, 4);
            echiquier.PlacerPiece(tourBlanche, 7, 7);

            var coupRoque = new Coup(roiBlanc, 7, 4, 7, 6)
            {
                EstPetitRoque = true
            };

            // Act
            bool estValide = _validateur.EstCoupValide(coupRoque, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void GrandRoqueNoir_EstValide_DansPositionSimple()
        {
            // Arrange
            var echiquier = CreerEchiquierAvecEtat(out var etat);
            var roiNoir = new Roi(CouleurPiece.Noir, 0, 4);
            var tourNoire = new Tour(CouleurPiece.Noir, 0, 0);

            echiquier.PlacerPiece(roiNoir, 0, 4);
            echiquier.PlacerPiece(tourNoire, 0, 0);

            var coupRoque = new Coup(roiNoir, 0, 4, 0, 2)
            {
                EstGrandRoque = true
            };

            // Act
            bool estValide = _validateur.EstCoupValide(coupRoque, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void PriseEnPassant_Blanche_EstValide_ApresDoublePasNoir()
        {
            // Arrange
            var echiquier = CreerEchiquierAvecEtat(out var etat);

            // Pion noir vient de jouer de 1 a 3 (ligne 1 -> 3, colonne 4)
            var pionNoir = new Pion(CouleurPiece.Noir, 3, 4);
            echiquier.PlacerPiece(pionNoir, 3, 4);

            var pionBlanc = new Pion(CouleurPiece.Blanc, 3, 3);
            echiquier.PlacerPiece(pionBlanc, 3, 3);

            var dernierCoup = new Coup(pionNoir, 1, 4, 3, 4);
            etat.HistoriqueCoups.Add(dernierCoup);
            echiquier.EtatPartie = etat;

            // Coup de prise en passant : le pion blanc va en (2,4)
            var coupEnPassant = new Coup(pionBlanc, 3, 3, 2, 4)
            {
                EstEnPassant = true
            };

            // Act
            bool estValide = _validateur.EstCoupValide(coupEnPassant, echiquier);

            // Assert
            Assert.True(estValide);
        }

        [Fact]
        public void PromotionPionBlanc_EstDetecteeEnArrivantHuitiemeRang()
        {
            // Arrange
            var echiquier = CreerEchiquierAvecEtat(out var etat);
            var pionBlanc = new Pion(CouleurPiece.Blanc, 1, 0);
            echiquier.PlacerPiece(pionBlanc, 1, 0);

            var coup = new Coup(pionBlanc, 1, 0, 0, 0);

            // Act
            bool peutEtrePromu = pionBlanc.PeutEtrePromu();

            // Assert
            Assert.True(peutEtrePromu);
        }
    }
}
