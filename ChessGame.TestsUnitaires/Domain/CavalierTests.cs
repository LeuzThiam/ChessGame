using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using Xunit;

namespace ChessGame.TestsUnitaires.Domain;

public class CavalierTests
{
    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 2)]
    [InlineData(-2, -1)]
    [InlineData(-1, -2)]
    public void EstCoupValide_ShouldReturnTrue_ForValidLShapeMove(int deltaLigne, int deltaColonne)
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        echiquier.PlacerPiece(cavalier, 4, 4);

        var resultat = cavalier.EstCoupValide(4 + deltaLigne, 4 + deltaColonne, echiquier);

        Assert.True(resultat);
    }

    [Theory]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(4, 4)]
    [InlineData(6, 6)]
    public void EstCoupValide_ShouldReturnFalse_ForInvalidPattern(int ligneDestination, int colonneDestination)
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        echiquier.PlacerPiece(cavalier, 4, 4);

        var resultat = cavalier.EstCoupValide(ligneDestination, colonneDestination, echiquier);

        Assert.False(resultat);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(8, 3)]
    [InlineData(2, -1)]
    [InlineData(3, 8)]
    public void EstCoupValide_ShouldReturnFalse_WhenDestinationIsOutOfBoard(int ligneDestination, int colonneDestination)
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        echiquier.PlacerPiece(cavalier, 4, 4);

        var resultat = cavalier.EstCoupValide(ligneDestination, colonneDestination, echiquier);

        Assert.False(resultat);
    }

    [Fact]
    public void EstCoupValide_ShouldReturnFalse_WhenDestinationContainsAllyPiece()
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        var pionAllie = new Pion(CouleurPiece.Blanc, 6, 5);

        echiquier.PlacerPiece(cavalier, 4, 4);
        echiquier.PlacerPiece(pionAllie, 6, 5);

        var resultat = cavalier.EstCoupValide(6, 5, echiquier);

        Assert.False(resultat);
    }

    [Fact]
    public void EstCoupValide_ShouldReturnTrue_WhenDestinationContainsEnemyPiece()
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        var pionAdverse = new Pion(CouleurPiece.Noir, 6, 5);

        echiquier.PlacerPiece(cavalier, 4, 4);
        echiquier.PlacerPiece(pionAdverse, 6, 5);

        var resultat = cavalier.EstCoupValide(6, 5, echiquier);

        Assert.True(resultat);
    }

    [Fact]
    public void ObtenirCoupsPossibles_ShouldReturn8Moves_WhenKnightIsAtCenterAndBoardIsEmpty()
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        echiquier.PlacerPiece(cavalier, 4, 4);

        var coups = cavalier.ObtenirCoupsPossibles(echiquier);

        Assert.Equal(8, coups.Count);
        Assert.Contains(coups, c => c.LigneArrivee == 2 && c.ColonneArrivee == 3);
        Assert.Contains(coups, c => c.LigneArrivee == 2 && c.ColonneArrivee == 5);
        Assert.Contains(coups, c => c.LigneArrivee == 3 && c.ColonneArrivee == 2);
        Assert.Contains(coups, c => c.LigneArrivee == 3 && c.ColonneArrivee == 6);
        Assert.Contains(coups, c => c.LigneArrivee == 5 && c.ColonneArrivee == 2);
        Assert.Contains(coups, c => c.LigneArrivee == 5 && c.ColonneArrivee == 6);
        Assert.Contains(coups, c => c.LigneArrivee == 6 && c.ColonneArrivee == 3);
        Assert.Contains(coups, c => c.LigneArrivee == 6 && c.ColonneArrivee == 5);
    }

    [Fact]
    public void ObtenirCoupsPossibles_ShouldReturn2Moves_WhenKnightIsAtCorner()
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 0, 0);
        echiquier.PlacerPiece(cavalier, 0, 0);

        var coups = cavalier.ObtenirCoupsPossibles(echiquier);

        Assert.Equal(2, coups.Count);
        Assert.Contains(coups, c => c.LigneArrivee == 1 && c.ColonneArrivee == 2);
        Assert.Contains(coups, c => c.LigneArrivee == 2 && c.ColonneArrivee == 1);
    }

    [Fact]
    public void ObtenirCoupsPossibles_ShouldExcludeAllyAndIncludeEnemy()
    {
        var echiquier = new Echiquier();
        var cavalier = new Cavalier(CouleurPiece.Blanc, 4, 4);
        var pionAllie = new Pion(CouleurPiece.Blanc, 2, 3);
        var pionAdverse = new Pion(CouleurPiece.Noir, 6, 5);

        echiquier.PlacerPiece(cavalier, 4, 4);
        echiquier.PlacerPiece(pionAllie, 2, 3);
        echiquier.PlacerPiece(pionAdverse, 6, 5);

        var coups = cavalier.ObtenirCoupsPossibles(echiquier);

        Assert.DoesNotContain(coups, c => c.LigneArrivee == 2 && c.ColonneArrivee == 3);

        var coupCapture = Assert.Single(coups, c => c.LigneArrivee == 6 && c.ColonneArrivee == 5);
        Assert.NotNull(coupCapture.PieceCapturee);
        Assert.Equal(CouleurPiece.Noir, coupCapture.PieceCapturee!.Couleur);
    }
}
