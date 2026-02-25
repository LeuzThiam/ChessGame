using System.Net.Http.Json;

namespace ChessGameMAUI.Services;

public class PieceDto
{
    public int Ligne { get; set; }
    public int Colonne { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Couleur { get; set; } = string.Empty;
    public bool ADejaBouge { get; set; }
}

public class MoveDto
{
    public int LigneDepart { get; set; }
    public int ColonneDepart { get; set; }
    public int LigneArrivee { get; set; }
    public int ColonneArrivee { get; set; }
    public string? Promotion { get; set; }
}

public class GameStateDto
{
    public string PartieId { get; set; } = string.Empty;
    public string JoueurActif { get; set; } = string.Empty;
    public string StatutPartie { get; set; } = string.Empty;
    public IList<PieceDto> Pieces { get; set; } = new List<PieceDto>();
    public IList<MoveDto> Historique { get; set; } = new List<MoveDto>();
}

public interface IChessApiClient
{
    Task<GameStateDto> CreerPartieAsync(string nomBlanc, string nomNoir, string modeJeu);
    Task<GameStateDto> ObtenirEtatAsync(string partieId);
    Task<IReadOnlyList<MoveDto>> ObtenirHistoriqueAsync(string partieId);
    Task<GameStateDto> JouerCoupAsync(string partieId, int ld, int cd, int la, int ca, string? promotion = null);
}

public class ChessApiClient : IChessApiClient
{
    private readonly HttpClient _http;

    public ChessApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<GameStateDto> CreerPartieAsync(string nomBlanc, string nomNoir, string modeJeu)
    {
        var req = new
        {
            nomJoueurBlanc = nomBlanc,
            nomJoueurNoir = nomNoir,
            modeJeu
        };

        var response = await _http.PostAsJsonAsync("api/games", req);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<GameStateDto> ObtenirEtatAsync(string partieId)
    {
        var response = await _http.GetAsync($"api/games/{partieId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }

    public async Task<IReadOnlyList<MoveDto>> ObtenirHistoriqueAsync(string partieId)
    {
        var response = await _http.GetAsync($"api/games/{partieId}/history");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<MoveDto>>())!;
    }

    public async Task<GameStateDto> JouerCoupAsync(string partieId, int ld, int cd, int la, int ca, string? promotion = null)
    {
        var body = new
        {
            ligneDepart = ld,
            colonneDepart = cd,
            ligneArrivee = la,
            colonneArrivee = ca,
            promotion
        };

        var response = await _http.PostAsJsonAsync($"api/games/{partieId}/moves", body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GameStateDto>())!;
    }
}
