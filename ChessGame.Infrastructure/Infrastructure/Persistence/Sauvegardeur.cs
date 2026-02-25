using ChessGame.Core.Domain.Models;
using ChessGame.Core.Domain.Models.Pieces;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Infrastructure.Persistence
{
    /// <summary>
    /// Service responsable de la sauvegarde et du chargement des parties d'échecs
    /// (PGN et FEN uniquement pour alléger le code).
    /// </summary>
    public class Sauvegardeur : ISauvegardeur
    {
        // ============================================================
        // ======================   PGN   ==============================
        // ============================================================

        public bool SauvegarderPGN(Echiquier echiquier, EtatPartie etatPartie, string cheminFichier)
        {
            try
            {
                string pgn = ExporterVersPGN(echiquier, etatPartie);
                File.WriteAllText(cheminFichier, pgn, Encoding.UTF8);
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
                string pgn = ExporterVersPGN(echiquier, etatPartie);
                await File.WriteAllTextAsync(cheminFichier, pgn, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ChargerPGN(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
                return null;

            try
            {
                string pgn = File.ReadAllText(cheminFichier, Encoding.UTF8);
                return ImporterDepuisPGN(pgn);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(Echiquier echiquier, EtatPartie etatPartie)?> ChargerPGNAsync(string cheminFichier)
        {
            if (!File.Exists(cheminFichier))
                return null;

            try
            {
                string pgn = await File.ReadAllTextAsync(cheminFichier, Encoding.UTF8);
                return ImporterDepuisPGN(pgn);
            }
            catch
            {
                return null;
            }
        }

        public string ExporterVersPGN(Echiquier echiquier, EtatPartie etatPartie)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"[Event \"ChessGame\"]");
            sb.AppendLine($"[Site \"Application WPF\"]");
            sb.AppendLine($"[Date \"{etatPartie.DateDebut:yyyy.MM.dd}\"]");
            sb.AppendLine($"[White \"{etatPartie.JoueurBlanc?.Nom ?? "White"}\"]");
            sb.AppendLine($"[Black \"{etatPartie.JoueurNoir?.Nom ?? "Black"}\"]");

            string resultat =
                etatPartie.Gagnant == null ? "*" :
                etatPartie.Gagnant.Couleur == CouleurPiece.Blanc ? "1-0" : "0-1";

            sb.AppendLine($"[Result \"{resultat}\"]");
            sb.AppendLine();

            // Export de l'historique
            for (int i = 0; i < etatPartie.HistoriqueCoups.Count; i++)
            {
                if (i % 2 == 0)
                    sb.Append($"{i / 2 + 1}. ");

                sb.Append(etatPartie.HistoriqueCoups[i].NotationAlgebrique + " ");
            }

            sb.AppendLine();
            sb.AppendLine(resultat);

            return sb.ToString();
        }

        public (Echiquier echiquier, EtatPartie etatPartie)? ImporterDepuisPGN(string pgnContent)
        {
            try
            {
                // Ce parser minimal NE rejoue PAS les coups.
                // Il charge une partie vide avec les deux joueurs.

                var headers = ParserEntetesPGN(pgnContent);

                string white = headers.GetValueOrDefault("White", "White");
                string black = headers.GetValueOrDefault("Black", "Black");

                var joueurBlanc = new Joueur(white, CouleurPiece.Blanc);
                var joueurNoir = new Joueur(black, CouleurPiece.Noir);

                var etat = new EtatPartie(joueurBlanc, joueurNoir);
                var echiquier = new Echiquier(etat);
                echiquier.InitialiserPositionStandard();

                return (echiquier, etat);
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, string> ParserEntetesPGN(string pgn)
        {
            var headers = new Dictionary<string, string>();

            foreach (string line in pgn.Split('\n'))
            {
                string t = line.Trim();
                if (t.StartsWith("[") && t.EndsWith("]"))
                {
                    string inside = t.Substring(1, t.Length - 2);
                    int i = inside.IndexOf(' ');
                    if (i < 0) continue;

                    string key = inside[..i];
                    string value = inside[(i + 1)..].Trim('"');

                    headers[key] = value;
                }
            }

            return headers;
        }

        // ============================================================
        // ======================   FEN   ==============================
        // ============================================================

        public bool SauvegarderFEN(Echiquier echiquier, string cheminFichier)
        {
            try
            {
                File.WriteAllText(cheminFichier, echiquier.VersNotationFEN(), Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Echiquier? ChargerFEN(string cheminFichier)
        {
            try
            {
                if (!File.Exists(cheminFichier))
                    return null;

                string fen = File.ReadAllText(cheminFichier, Encoding.UTF8);
                return ImporterDepuisFEN(fen);
            }
            catch
            {
                return null;
            }
        }

        public string ExporterVersFEN(Echiquier echiquier, EtatPartie etatPartie)
        {
            string fenBase = echiquier.VersNotationFEN();
            string actif = etatPartie.JoueurActif?.Couleur == CouleurPiece.Noir ? "b" : "w";

            return $"{fenBase} {actif} - - 0 1";
        }

        public Echiquier? ImporterDepuisFEN(string fenString)
        {
            try
            {
                string[] parts = fenString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string disposition = parts[0];

                var joueurB = new Joueur("White", CouleurPiece.Blanc);
                var joueurN = new Joueur("Black", CouleurPiece.Noir);
                var etat = new EtatPartie(joueurB, joueurN);
                var echiquier = new Echiquier(etat);
                echiquier.Vider();

                string[] rangs = disposition.Split('/');

                for (int r = 0; r < 8; r++)
                {
                    int c = 0;
                    foreach (char ch in rangs[r])
                    {
                        if (char.IsDigit(ch))
                        {
                            c += ch - '0';
                            continue;
                        }

                        Piece? p = PieceDepuisSymbole(ch, r, c);
                        if (p != null)
                            echiquier.PlacerPiece(p, r, c);

                        c++;
                    }
                }

                return echiquier;
            }
            catch
            {
                return null;
            }
        }

        private Piece? PieceDepuisSymbole(char s, int lig, int col)
        {
            CouleurPiece colPiece = char.IsUpper(s) ? CouleurPiece.Blanc : CouleurPiece.Noir;
            char t = char.ToLower(s);

            return t switch
            {
                'p' => new Pion(colPiece, lig, col),
                'r' => new Tour(colPiece, lig, col),
                'n' => new Cavalier(colPiece, lig, col),
                'b' => new Fou(colPiece, lig, col),
                'q' => new Reine(colPiece, lig, col),
                'k' => new Roi(colPiece, lig, col),
                _ => null
            };
        }
    }
}
