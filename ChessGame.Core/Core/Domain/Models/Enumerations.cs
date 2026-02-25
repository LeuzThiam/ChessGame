namespace ChessGame.Core.Domain.Models
{
    /// <summary>
    /// Couleur d'une pièce d'échecs
    /// </summary>
    public enum CouleurPiece
    {
        Blanc,
        Noir
    }

    /// <summary>
    /// Type de pièce d'échecs
    /// </summary>
    public enum TypePiece
    {
        Pion,
        Tour,
        Cavalier,
        Fou,
        Reine,
        Roi,
        Aucune
    }

    /// <summary>
    /// Statut de la partie
    /// </summary>
    public enum StatutPartie
    {
        EnCours,
        EchecBlanc,
        EchecNoir,
        EchecEtMatBlanc,
        EchecEtMatNoir,
        Pat,
        Nulle,
        AbandonBlanc,
        AbandonNoir
    }

    /// <summary>
    /// Type de fin de partie
    /// </summary>
    public enum TypeFinPartie
    {
        Aucune,
        EchecEtMat,
        Pat,
        Abandon,
        Nulle,
        RegleDes50Coups,
        RepetitionTriple,
        MateriellementImpossible
    }

    /// <summary>
    /// Type de coup spécial
    /// </summary>
    public enum TypeCoup
    {
        Normal,
        Capture,
        PetitRoque,
        GrandRoque,
        EnPassant,
        Promotion
    }
}