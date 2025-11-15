using ChessGame.Models;
using ChessGame.Models.Pieces;
using ChessGame.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

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
            var sb = new StringBuilder();

            sb.Append(echiquier.VersNotationFEN());
            sb.Append(" ");
            sb.Append(etatPartie.JoueurActif?.Couleur == CouleurPiece.Blanc ? "w" : "b");
            sb.Append(" ");

            string roques = "";
            if (etatPartie.JoueurBlanc.PeutRoquerPetit) roques += "K";
            if (etatPartie.JoueurBlanc.PeutRoquerGrand) roques += "Q";
            if (etatPartie.JoueurNoir.PeutRoquerPetit) roques += "k";
            if (etatPartie.JoueurNoir.PeutRoquerGrand) roques += "q";
            sb.Append(roques == "" ? "-" : roques);
            sb.Append(" ");

            // En passant
            sb.Append("- ");
            sb.Append(etatPartie.CompteurDemiCoups);
            sb.Append(" ");
            sb.Append(etatPartie.NumeroCoup);

            return sb.ToString();
        }

        public Echiquier ImporterDepuisFEN(string fenString)
        {
            try
            {
                if (!ValiderFEN(fenString))
                    return null;

                string[] sections = fenString.Split(' ');
                string disposition = sections[0];

                Echiquier echiquier = new();
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
                var data = new
                {
                    FEN = ExporterVersFEN(echiquier, etatPartie),
                    JoueurBlanc = etatPartie.JoueurBlanc.Nom,
                    JoueurNoir = etatPartie.JoueurNoir.Nom,
                    Date = etatPartie.DateDebut,
                    Statut = etatPartie.Statut.ToString(),
                    Coups = etatPartie.HistoriqueCoups.Select(c => c.NotationAlgebrique).ToList()
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(cheminFichier, json, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ChargerJSON(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string json = File.ReadAllText(cheminFichier);

                // ❗ Parser JSON COMPLET à implémenter ici
                return null;
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

            string[] parts = fenString.Split(' ');
            if (parts.Length < 1)
                return false;

            string[] rows = parts[0].Split('/');
            return rows.Length == 8;
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

        // ----------------------------------------------------------------------
        #endregion
    }
}
