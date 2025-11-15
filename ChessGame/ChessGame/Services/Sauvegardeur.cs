using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChessGame.Services
{
    /// <summary>
    /// Service responsable de la sauvegarde et du chargement des parties d'échecs
    /// Formats supportés : PGN, FEN, JSON
    /// </summary>
    public class Sauvegardeur : ISauvegardeur
    {
        private System.Timers.Timer _autoSaveTimer;
        private string _autoSaveFolder;

        // ----------------------------------------------------------------------
        #region ───── SAUVEGARDE / CHARGEMENT PGN ──────────────────────────────
        // ----------------------------------------------------------------------

        public bool SauvegarderPGN(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier)
        {
            try
            {
                string pgnContent = ExporterVersPGN(echiquier, etatPartie);
                File.WriteAllText(cheminFichier, pgnContent, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SauvegarderPGNAsync(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier)
        {
            try
            {
                string pgnContent = ExporterVersPGN(echiquier, etatPartie);
                await File.WriteAllTextAsync(cheminFichier, pgnContent, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ChargerPGN(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string pgnContent = File.ReadAllText(cheminFichier, Encoding.UTF8);
                return ImporterDepuisPGN(pgnContent);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(Echiquier echiquier, EtatPartie etatPartie)?> ChargerPGNAsync(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string pgnContent = await File.ReadAllTextAsync(cheminFichier, Encoding.UTF8);
                return ImporterDepuisPGN(pgnContent);
            }
            catch
            {
                return null;
            }
        }

        public string ExporterVersPGN(Echiquier echiquier, EtatPartie etatPartie)
        {
            StringBuilder pgn = new();

            // En-têtes PGN
            pgn.AppendLine($"[Event \"{etatPartie.TypeFin}\"]");
            pgn.AppendLine($"[Site \"ChessGame Application\"]");
            pgn.AppendLine($"[Date \"{etatPartie.DateDebut:yyyy.MM.dd}\"]");
            pgn.AppendLine($"[Round \"1\"]");
            pgn.AppendLine($"[White \"{etatPartie.JoueurBlanc?.Nom ?? "White"}\"]");
            pgn.AppendLine($"[Black \"{etatPartie.JoueurNoir?.Nom ?? "Black"}\"]");

            string resultat = DeterminerResultatPGN(etatPartie);
            pgn.AppendLine($"[Result \"{resultat}\"]");

            pgn.AppendLine();

            // Export des coups (si déjà notés)
            for (int i = 0; i < etatPartie.HistoriqueCoups.Count; i++)
            {
                if (i % 2 == 0)
                    pgn.Append($"{(i / 2) + 1}. ");

                pgn.Append(etatPartie.HistoriqueCoups[i].NotationAlgebrique + " ");

                if ((i + 1) % 6 == 0)
                    pgn.AppendLine();
            }

            pgn.AppendLine(resultat);

            return pgn.ToString();
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisPGN(string pgnContent)
        {
            try
            {
                Dictionary<string, string> headers = ParserEntetesPGN(pgnContent);

                string w = headers.GetValueOrDefault("White", "White");
                string b = headers.GetValueOrDefault("Black", "Black");

                Joueur joueurBlanc = new(w, CouleurPiece.Blanc);
                Joueur joueurNoir = new(b, CouleurPiece.Noir);

                EtatPartie etat = new(joueurBlanc, joueurNoir);

                if (headers.TryGetValue("Date", out string dateStr))
                {
                    if (DateTime.TryParse(dateStr.Replace(".", "/"), out DateTime d))
                        etat.DateDebut = d;
                }

                Echiquier echiquier = new(etat);
                echiquier.InitialiserPositionStandard();

                // ❗ Implémentation simplifiée :
                // Le parser PGN complet doit rejouer les coups.
                // Ici on charge juste la position initiale.

                return (echiquier, etat);
            }
            catch
            {
                return null;
            }
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── SAUVEGARDE / CHARGEMENT FEN ─────────────────────────────
        // ----------------------------------------------------------------------

        public bool SauvegarderFEN(Echiquier echiquier, string cheminFichier)
        {
            try
            {
                string fen = echiquier.VersNotationFEN();
                File.WriteAllText(cheminFichier, fen, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Echiquier ChargerFEN(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string content = File.ReadAllText(cheminFichier, Encoding.UTF8);
                return ImporterDepuisFEN(content);
            }
            catch
            {
                return null;
            }
        }

        public string ExporterVersFEN(Echiquier echiquier, EtatPartie etatPartie)
        {
            if (echiquier == null)
                throw new ArgumentNullException(nameof(echiquier));

            var sb = new StringBuilder();

            sb.Append(echiquier.VersNotationFEN());
            sb.Append(" ");
            var joueurActif = etatPartie?.JoueurActif?.Couleur == CouleurPiece.Noir ? "b" : "w";
            sb.Append(joueurActif);
            sb.Append(" ");

            string roques = "";
            var joueurBlanc = etatPartie?.JoueurBlanc;
            var joueurNoir = etatPartie?.JoueurNoir;

            if (joueurBlanc?.PeutRoquerPetit == true) roques += "K";
            if (joueurBlanc?.PeutRoquerGrand == true) roques += "Q";
            if (joueurNoir?.PeutRoquerPetit == true) roques += "k";
            if (joueurNoir?.PeutRoquerGrand == true) roques += "q";
            sb.Append(roques == "" ? "-" : roques);
            sb.Append(" ");

            // En passant
            sb.Append("- ");
            sb.Append(etatPartie?.CompteurDemiCoups ?? 0);
            sb.Append(" ");
            sb.Append(etatPartie?.NumeroCoup ?? 1);

            return sb.ToString();
        }

        public Echiquier ImporterDepuisFEN(string fenString)
        {
            try
            {
                if (!ValiderFEN(fenString))
                    return null;

                string[] sections = fenString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string disposition = sections[0];

                Joueur joueurBlanc = new("White", CouleurPiece.Blanc);
                Joueur joueurNoir = new("Black", CouleurPiece.Noir);
                EtatPartie etatPartie = new(joueurBlanc, joueurNoir);

                Echiquier echiquier = new(etatPartie);
                echiquier.Vider();

                string[] rangées = disposition.Split('/');

                for (int r = 0; r < 8; r++)
                {
                    int c = 0;
                    foreach (char ch in rangées[r])
                    {
                        if (char.IsDigit(ch))
                        {
                            c += ch - '0';
                        }
                        else
                        {
                            Piece p = CreerPieceDepuisSymbole(ch, r, c);
                            if (p != null)
                                echiquier.PlacerPiece(p, r, c);

                            c++;
                        }
                    }
                }

                if (sections.Length > 1)
                {
                    if (sections[1] == "b")
                    {
                        etatPartie.JoueurActif = joueurNoir;
                        joueurBlanc.EstSonTour = false;
                        joueurNoir.EstSonTour = true;
                    }
                    else
                    {
                        etatPartie.JoueurActif = joueurBlanc;
                        joueurBlanc.EstSonTour = true;
                        joueurNoir.EstSonTour = false;
                    }
                }

                if (sections.Length > 2)
                {
                    string roques = sections[2];

                    if (roques == "-")
                    {
                        joueurBlanc.PeutRoquerPetit = false;
                        joueurBlanc.PeutRoquerGrand = false;
                        joueurNoir.PeutRoquerPetit = false;
                        joueurNoir.PeutRoquerGrand = false;
                    }
                    else
                    {
                        joueurBlanc.PeutRoquerPetit = roques.Contains('K');
                        joueurBlanc.PeutRoquerGrand = roques.Contains('Q');
                        joueurNoir.PeutRoquerPetit = roques.Contains('k');
                        joueurNoir.PeutRoquerGrand = roques.Contains('q');
                    }
                }

                if (sections.Length > 4 && int.TryParse(sections[4], out int compteurDemiCoups))
                    etatPartie.CompteurDemiCoups = compteurDemiCoups;

                if (sections.Length > 5 && int.TryParse(sections[5], out int numeroCoup))
                    etatPartie.NumeroCoup = Math.Max(1, numeroCoup);

                return echiquier;
            }
            catch
            {
                return null;
            }
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── SAUVEGARDE / CHARGEMENT JSON ────────────────────────────
        // ----------------------------------------------------------------------

        public bool SauvegarderJSON(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier)
        {
            try
            {
                string json = ExporterVersJSON(echiquier, etatPartie);
                File.WriteAllText(cheminFichier, json, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ExporterVersJSON(Echiquier echiquier, EtatPartie etatPartie)
        {
            if (echiquier == null)
                throw new ArgumentNullException(nameof(echiquier));

            EtatPartie etat = etatPartie ?? echiquier?.EtatPartie;

            var data = new PartieJsonData
            {
                FEN = ExporterVersFEN(echiquier, etat),
                JoueurBlanc = etat?.JoueurBlanc?.Nom,
                JoueurNoir = etat?.JoueurNoir?.Nom,
                JoueurActif = etat?.JoueurActif?.Couleur.ToString(),
                Date = etat?.DateDebut,
                Statut = etat?.Statut.ToString(),
                TypeFin = etat?.TypeFin.ToString(),
                Gagnant = etat?.Gagnant?.Couleur.ToString(),
                NumeroCoup = etat?.NumeroCoup,
                CompteurDemiCoups = etat?.CompteurDemiCoups,
                Coups = etat?.HistoriqueCoups?.Select(c => c.NotationAlgebrique).ToList() ?? new List<string>()
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ChargerJSON(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string json = File.ReadAllText(cheminFichier);

                return ImporterDepuisJSON(json);
            }
            catch
            {
                return null;
            }
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisJSON(string jsonContent)
        {
            try
            {
                var data = JsonSerializer.Deserialize<PartieJsonData>(jsonContent);
                if (data == null)
                    return null;

                Echiquier echiquier = null;
                EtatPartie etatPartie = null;

                if (!string.IsNullOrWhiteSpace(data.FEN))
                {
                    echiquier = ImporterDepuisFEN(data.FEN);
                    etatPartie = echiquier?.EtatPartie;
                }

                if (echiquier == null || etatPartie == null)
                {
                    Joueur joueurBlanc = new(data.JoueurBlanc ?? "White", CouleurPiece.Blanc);
                    Joueur joueurNoir = new(data.JoueurNoir ?? "Black", CouleurPiece.Noir);
                    etatPartie = new EtatPartie(joueurBlanc, joueurNoir);
                    echiquier = new Echiquier(etatPartie);
                }

                if (!string.IsNullOrWhiteSpace(data.JoueurBlanc))
                    etatPartie.JoueurBlanc.Nom = data.JoueurBlanc;
                if (!string.IsNullOrWhiteSpace(data.JoueurNoir))
                    etatPartie.JoueurNoir.Nom = data.JoueurNoir;

                if (data.Date.HasValue)
                    etatPartie.DateDebut = data.Date.Value;

                if (!string.IsNullOrWhiteSpace(data.Statut) && Enum.TryParse(data.Statut, true, out StatutPartie statut))
                    etatPartie.Statut = statut;

                if (!string.IsNullOrWhiteSpace(data.TypeFin) && Enum.TryParse(data.TypeFin, true, out TypeFinPartie typeFin))
                    etatPartie.TypeFin = typeFin;

                if (!string.IsNullOrWhiteSpace(data.Gagnant) && Enum.TryParse(data.Gagnant, true, out CouleurPiece couleurGagnant))
                    etatPartie.Gagnant = couleurGagnant == CouleurPiece.Blanc ? etatPartie.JoueurBlanc : etatPartie.JoueurNoir;

                if (!string.IsNullOrWhiteSpace(data.JoueurActif) && Enum.TryParse(data.JoueurActif, true, out CouleurPiece couleurActif))
                    etatPartie.JoueurActif = couleurActif == CouleurPiece.Blanc ? etatPartie.JoueurBlanc : etatPartie.JoueurNoir;

                etatPartie.JoueurBlanc.EstSonTour = etatPartie.JoueurActif == etatPartie.JoueurBlanc;
                etatPartie.JoueurNoir.EstSonTour = etatPartie.JoueurActif == etatPartie.JoueurNoir;

                if (data.NumeroCoup.HasValue && data.NumeroCoup.Value > 0)
                    etatPartie.NumeroCoup = data.NumeroCoup.Value;

                if (data.CompteurDemiCoups.HasValue && data.CompteurDemiCoups.Value >= 0)
                    etatPartie.CompteurDemiCoups = data.CompteurDemiCoups.Value;

                return (echiquier, etatPartie);
            }
            catch
            {
                return null;
            }
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── MÉTADONNÉES ─────────────────────────────────────────────
        // ----------------------------------------------------------------------

        public MetadonneesPartie ObtenirMetadonnees(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                FileInfo fi = new(cheminFichier);

                var meta = new MetadonneesPartie
                {
                    NomFichier = fi.Name,
                    CheminComplet = fi.FullName,
                    TailleFichier = fi.Length,
                    DateSauvegarde = fi.LastWriteTime,
                    Format = fi.Extension.ToLower() switch
                    {
                        ".pgn" => "PGN",
                        ".fen" => "FEN",
                        ".json" => "JSON",
                        _ => "Inconnu"
                    }
                };

                if (meta.Format == "PGN")
                {
                    var headers = ParserEntetesPGN(File.ReadAllText(cheminFichier));

                    meta.JoueurBlanc = headers.GetValueOrDefault("White", "?");
                    meta.JoueurNoir = headers.GetValueOrDefault("Black", "?");
                    meta.Resultat = headers.GetValueOrDefault("Result", "*");

                    if (headers.TryGetValue("Date", out string d))
                        DateTime.TryParse(d.Replace(".", "/"), out meta.DatePartie);
                }
                else if (meta.Format == "JSON")
                {
                    string content = File.ReadAllText(cheminFichier, Encoding.UTF8);
                    try
                    {
                        var data = JsonSerializer.Deserialize<PartieJsonData>(content);
                        if (data != null)
                        {
                            meta.JoueurBlanc = data.JoueurBlanc ?? "?";
                            meta.JoueurNoir = data.JoueurNoir ?? "?";
                            meta.NombreCoups = data.Coups?.Count ?? 0;

                            if (data.Date.HasValue)
                                meta.DatePartie = data.Date.Value;

                            meta.Resultat = DeterminerResultatDepuisJson(data) ?? "*";
                        }
                    }
                    catch
                    {
                        meta.JoueurBlanc = meta.JoueurBlanc ?? "?";
                        meta.JoueurNoir = meta.JoueurNoir ?? "?";
                        meta.Resultat = meta.Resultat ?? "*";
                    }
                }

                return meta;
            }
            catch
            {
                return null;
            }
        }

        public List<MetadonneesPartie> ListerPartiesSauvegardees(string cheminDossier)
        {
            List<MetadonneesPartie> list = new();

            try
            {
                if (!Directory.Exists(cheminDossier))
                    return list;

                var fichiers = Directory.GetFiles(cheminDossier, "*.*")
                    .Where(f => f.EndsWith(".pgn") || f.EndsWith(".fen") || f.EndsWith(".json"));

                foreach (var f in fichiers)
                {
                    var meta = ObtenirMetadonnees(f);
                    if (meta != null)
                        list.Add(meta);
                }

                return list.OrderByDescending(f => f.DateSauvegarde).ToList();
            }
            catch
            {
                return list;
            }
        }

        public List<MetadonneesPartie> ListerPartiesSauvegardeesAvecFiltres(
            string cheminDossier,
            string format = null,
            DateTime? dateDebut = null,
            DateTime? dateFin = null)
        {
            var parties = ListerPartiesSauvegardees(cheminDossier);

            if (!string.IsNullOrWhiteSpace(format))
            {
                string formatUpper = format.ToUpperInvariant();
                parties = parties
                    .Where(p => string.Equals(p.Format, formatUpper, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (dateDebut.HasValue)
                parties = parties.Where(p => p.DatePartie >= dateDebut.Value).ToList();

            if (dateFin.HasValue)
                parties = parties.Where(p => p.DatePartie <= dateFin.Value).ToList();

            return parties;
        }

        public List<MetadonneesPartie> RechercherPartiesParJoueur(string cheminDossier, string nomJoueur)
        {
            if (string.IsNullOrWhiteSpace(nomJoueur))
                return new List<MetadonneesPartie>();

            string terme = nomJoueur.Trim();

            return ListerPartiesSauvegardees(cheminDossier)
                .Where(meta =>
                    (!string.IsNullOrEmpty(meta.JoueurBlanc) &&
                     meta.JoueurBlanc.Contains(terme, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(meta.JoueurNoir) &&
                     meta.JoueurNoir.Contains(terme, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── VALIDATION ───────────────────────────────────────────────
        // ----------------------------------------------------------------------

        public bool ValiderFichierPGN(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
                return false;

            try
            {
                string content = File.ReadAllText(cheminFichier);
                var headers = ParserEntetesPGN(content);

                return new[]
                {
                    "Event","Site","Date","Round","White","Black","Result"
                }.All(h => headers.ContainsKey(h));
            }
            catch
            {
                return false;
            }
        }

        public bool ValiderFEN(string fenString)
        {
            if (string.IsNullOrWhiteSpace(fenString))
                return false;

            string[] parts = fenString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                return false;

            string[] rows = parts[0].Split('/');
            if (rows.Length != 8)
                return false;

            foreach (var row in rows)
            {
                int count = 0;
                foreach (char ch in row)
                {
                    if (char.IsDigit(ch))
                    {
                        int value = ch - '0';
                        if (value <= 0 || value > 8)
                            return false;

                        count += value;
                    }
                    else if ("prnbqkPRNBQK".IndexOf(ch) >= 0)
                    {
                        count++;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (count != 8)
                    return false;
            }

            if (parts.Length > 1 && parts[1] != "w" && parts[1] != "b")
                return false;

            if (parts.Length > 2)
            {
                string roques = parts[2];
                if (roques != "-" && roques.Any(c => !"KQkq".Contains(c)))
                    return false;
            }

            if (parts.Length > 4 && (!int.TryParse(parts[4], out int demi) || demi < 0))
                return false;

            if (parts.Length > 5 && (!int.TryParse(parts[5], out int numero) || numero <= 0))
                return false;

            return true;
        }

        public bool ValiderFichierJSON(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
                return false;

            try
            {
                string json = File.ReadAllText(cheminFichier);
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── UTILITAIRES FICHIERS ─────────────────────────────────────
        // ----------------------------------------------------------------------

        public bool SupprimerPartie(string cheminFichier)
        {
            try
            {
                if (File.Exists(cheminFichier))
                {
                    File.Delete(cheminFichier);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public bool RenommerPartie(string ancienChemin, string nouveauChemin)
        {
            try
            {
                if (!File.Exists(ancienChemin))
                    return false;

                File.Move(ancienChemin, nouveauChemin);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CopierPartie(string cheminSource, string cheminDestination)
        {
            try
            {
                File.Copy(cheminSource, cheminDestination, overwrite: true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public long ObtenirTailleFichier(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return 0;

                return new FileInfo(cheminFichier).Length;
            }
            catch
            {
                return 0;
            }
        }

        public bool FichierExiste(string cheminFichier) => File.Exists(cheminFichier);

        public string ObtenirExtension(string format)
        {
            return format.ToUpper() switch
            {
                "PGN" => ".pgn",
                "FEN" => ".fen",
                "JSON" => ".json",
                _ => ""
            };
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── SAUVEGARDE AUTOMATIQUE ───────────────────────────────────
        // ----------------------------------------------------------------------

        public void ActiverSauvegardeAutomatique(string dossier, int intervalleMinutes = 5)
        {
            if (!Directory.Exists(dossier))
                Directory.CreateDirectory(dossier);

            _autoSaveFolder = dossier;

            _autoSaveTimer = new System.Timers.Timer(intervalleMinutes * 60000);
            _autoSaveTimer.Elapsed += (s, e) =>
            {
                string file = Path.Combine(dossier, $"autosave_{DateTime.Now:yyyyMMdd_HHmmss}.json");

                // ❗ Pour l'instant : ne fait rien, la logique doit appeler SauvegarderJSON
            };
            _autoSaveTimer.Start();
        }

        public void DesactiverSauvegardeAutomatique()
        {
            _autoSaveTimer?.Stop();
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;
        }

        public bool EstSauvegardeAutomatiqueActive()
        {
            return _autoSaveTimer != null;
        }

        // ----------------------------------------------------------------------
        #endregion
        #region ───── MÉTHODES PRIVÉES ─────────────────────────────────────────
        // ----------------------------------------------------------------------

        private Dictionary<string, string> ParserEntetesPGN(string pgn)
        {
            Dictionary<string, string> headers = new();

            foreach (string line in pgn.Split('\n'))
            {
                string t = line.Trim();
                if (t.StartsWith("[") && t.EndsWith("]"))
                {
                    string inside = t.Substring(1, t.Length - 2);
                    int spaceIndex = inside.IndexOf(' ');
                    if (spaceIndex < 0) continue;

                    string key = inside[..spaceIndex];
                    string value = inside[(spaceIndex + 1)..].Trim('"');

                    headers[key] = value;
                }
            }

            return headers;
        }

        private string DeterminerResultatPGN(EtatPartie etat)
        {
            if (etat.Gagnant != null)
                return etat.Gagnant.Couleur == CouleurPiece.Blanc ? "1-0" : "0-1";

            if (etat.Statut is StatutPartie.Nulle or StatutPartie.Pat)
                return "1/2-1/2";

            return "*";
        }

        private string DeterminerResultatDepuisJson(PartieJsonData data)
        {
            if (!string.IsNullOrWhiteSpace(data.Gagnant) && Enum.TryParse(data.Gagnant, true, out CouleurPiece gagnant))
                return gagnant == CouleurPiece.Blanc ? "1-0" : "0-1";

            if (!string.IsNullOrWhiteSpace(data.Statut) && Enum.TryParse(data.Statut, true, out StatutPartie statut))
            {
                if (statut is StatutPartie.Nulle or StatutPartie.Pat)
                    return "1/2-1/2";
            }

            return null;
        }

        private Piece CreerPieceDepuisSymbole(char symbole, int ligne, int colonne)
        {
            CouleurPiece couleur = char.IsUpper(symbole) ? CouleurPiece.Blanc : CouleurPiece.Noir;
            char t = char.ToLower(symbole);

            return t switch
            {
                'p' => new Pion(couleur, ligne, colonne),
                'r' => new Tour(couleur, ligne, colonne),
                'n' => new Cavalier(couleur, ligne, colonne),
                'b' => new Fou(couleur, ligne, colonne),
                'q' => new Reine(couleur, ligne, colonne),
                'k' => new Roi(couleur, ligne, colonne),
                _ => null
            };
        }

        private class PartieJsonData
        {
            public string FEN { get; set; }
            public string JoueurBlanc { get; set; }
            public string JoueurNoir { get; set; }
            public string JoueurActif { get; set; }
            public DateTime? Date { get; set; }
            public string Statut { get; set; }
            public string TypeFin { get; set; }
            public string Gagnant { get; set; }
            public int? NumeroCoup { get; set; }
            public int? CompteurDemiCoups { get; set; }
            public List<string> Coups { get; set; } = new();
        }

        // ----------------------------------------------------------------------
        #endregion
    }
}
