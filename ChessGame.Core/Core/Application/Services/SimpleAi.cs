using ChessGame.Core.Domain.Models;
using ChessGame.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChessGame.Core.Application.Services
{
    /// <summary>
    /// IA améliorée : utilise Minimax pour les niveaux élevés, approche simple pour les niveaux bas.
    /// Niveau 1-2 : coups aléatoires
    /// Niveau 3-4 : privilégie les captures et coups évalués
    /// Niveau 5-6 : utilise Minimax avec profondeur croissante
    /// </summary>
    public class SimpleAi : ISimpleAi
    {
        private readonly Random _rng = new();
        private readonly IMoteurMinimax? _moteurMinimax;
        private readonly IEvaluationPosition? _evaluationPosition;
        private readonly GenerateurCoups? _generateurCoups;

        /// <summary>
        /// Constructeur sans dépendances (pour compatibilité)
        /// </summary>
        public SimpleAi()
        {
        }

        /// <summary>
        /// Constructeur avec dépendances pour utiliser Minimax
        /// </summary>
        public SimpleAi(
            IValidateurCoup validateurCoup,
            IReglesJeu reglesJeu,
            IEvaluationPosition evaluationPosition)
        {
            if (validateurCoup == null || reglesJeu == null || evaluationPosition == null)
                return; // Mode dégradé sans Minimax

            _evaluationPosition = evaluationPosition;
            _generateurCoups = new GenerateurCoups(validateurCoup);
            _moteurMinimax = new MoteurMinimax(validateurCoup, reglesJeu, evaluationPosition);
        }

        /// <summary>
        /// Attache l'IA à une partie pour jouer automatiquement.
        /// </summary>
        public void Attacher(IServicePartie service, CouleurPiece couleurIa, int niveau = 1)
        {
            var ai = new SimpleAiInstance(service, couleurIa, niveau, this, _moteurMinimax);
            ai.Hooker();
        }

        /// <summary>
        /// Choisit le meilleur coup selon le niveau de l'IA.
        /// </summary>
        public Coup ChoisirMeilleurCoup(List<Coup> coups, int niveau = 1)
        {
            if (coups == null || coups.Count == 0)
                throw new ArgumentException("La liste des coups ne peut pas être vide", nameof(coups));

            niveau = Math.Clamp(niveau, 1, 6);

            // Niveau 1-2 : aléatoire
            if (niveau <= 2)
                return coups[_rng.Next(coups.Count)];

            // Niveau 3-4 : privilégier les captures et évaluer les coups
            if (niveau <= 4)
            {
                // Trier par valeur de capture (MVV-LVA)
                var coupsTries = coups.OrderByDescending(c =>
                {
                    int score = 0;
                    if (c.PieceCapturee != null)
                    {
                        // Score = valeur pièce capturée * 10 - valeur pièce attaquante
                        score = c.PieceCapturee.Valeur * 10 - (c.Piece?.Valeur ?? 0);
                    }
                    // Bonus pour échec si on peut le détecter facilement
                    if (c.DonneEchec)
                        score += 50;
                    return score;
                }).ToList();

                // Pour le niveau 3, prendre parmi les 3 meilleurs
                // Pour le niveau 4, prendre parmi les 2 meilleurs
                int nombreMeilleurs = niveau == 3 ? Math.Min(3, coupsTries.Count) : Math.Min(2, coupsTries.Count);
                var meilleursCoups = coupsTries.Take(nombreMeilleurs).ToList();
                return meilleursCoups[_rng.Next(meilleursCoups.Count)];
            }

            // Niveau 5-6 : Minimax est géré dans SimpleAiInstance.JouerUnCoupAsync()
            // Ici on fait un tri intelligent comme fallback (si Minimax échoue)
            var coupsTriesFallback = coups.OrderByDescending(c =>
            {
                int score = 0;
                
                // MVV-LVA : captures de pièces précieuses par des pièces moins précieuses
                if (c.PieceCapturee != null)
                {
                    score = c.PieceCapturee.Valeur * 10 - (c.Piece?.Valeur ?? 0);
                }
                
                // Bonus pour échec
                if (c.DonneEchec)
                    score += 50;
                
                // Pénalité pour perdre une pièce précieuse
                // (on ne peut pas vraiment évaluer sans simuler, mais on essaie)
                
                return score;
            }).ToList();

            // Pour le niveau 5, prendre parmi les 2 meilleurs
            // Pour le niveau 6, prendre le meilleur
            int nombreMeilleursFallback = niveau == 5 ? Math.Min(2, coupsTriesFallback.Count) : 1;
            var meilleursCoupsFallback = coupsTriesFallback.Take(nombreMeilleursFallback).ToList();
            return meilleursCoupsFallback[_rng.Next(meilleursCoupsFallback.Count)];
        }


        private class SimpleAiInstance
        {
            private readonly IServicePartie _service;
            private readonly CouleurPiece _couleurIa;
            private readonly int _niveau;
            private bool _enCours;
            private readonly Random _rng = new();
            private readonly SimpleAi _aiService;
            private readonly IMoteurMinimax? _moteurMinimax;

            public SimpleAiInstance(IServicePartie service, CouleurPiece couleurIa, int niveau, SimpleAi aiService, IMoteurMinimax? moteurMinimax)
            {
                _service = service;
                _couleurIa = couleurIa;
                _niveau = Math.Clamp(niveau, 1, 6);
                _aiService = aiService;
                _moteurMinimax = moteurMinimax;
            }

            public void Hooker()
            {
                Console.WriteLine($"[IA] Hooker() - Attachement de l'IA pour la couleur {_couleurIa}, niveau {_niveau}");
                
                _service.CoupJoue += OnCoupChange;
                _service.CoupAnnule += OnCoupChange;
                _service.PartieTerminee += (s, e) => {
                    Console.WriteLine($"[IA] Partie terminée détectée");
                    _enCours = false;
                };

                // Si l'IA commence
                var joueurActif = _service.ObtenirJoueurActif();
                Console.WriteLine($"[IA] Joueur actif au démarrage: {joueurActif?.Couleur}");
                
                if (joueurActif?.Couleur == _couleurIa)
                {
                    Console.WriteLine($"[IA] Je commence la partie !");
                    _ = JouerUnCoupAsync();
                }
                else
                {
                    Console.WriteLine($"[IA] J'attends que l'adversaire joue.");
                }
            }

            private void OnCoupChange(object? sender, Coup e)
            {
                var joueurActif = _service.ObtenirJoueurActif();
                Console.WriteLine($"[IA] Coup joué détecté. Joueur actif: {joueurActif?.Couleur}, IA attend: {_couleurIa}");
                
                if (joueurActif?.Couleur == _couleurIa)
                {
                    Console.WriteLine($"[IA] C'est mon tour ! Je joue...");
                    _ = JouerUnCoupAsync();
                }
                else
                {
                    Console.WriteLine($"[IA] Pas mon tour, j'attends...");
                }
            }

            private async Task JouerUnCoupAsync()
            {
                Console.WriteLine($"[IA] JouerUnCoupAsync appelé. EnCours: {_enCours}, Terminée: {_service.EstPartieTerminee()}");
                
                if (_enCours || _service.EstPartieTerminee())
                {
                    Console.WriteLine($"[IA] Sortie anticipée (enCours={_enCours}, terminée={_service.EstPartieTerminee()})");
                    return;
                }

                _enCours = true;
                
                // Délai plus long pour les niveaux élevés (Minimax prend du temps)
                int delai = _niveau >= 5 ? 300 : 200;
                Console.WriteLine($"[IA] Attente de {delai}ms...");
                await Task.Delay(delai);

                Coup? coup = null;

                // Pour les niveaux 5-6, essayer Minimax avec un timeout court
                // Si Minimax échoue ou timeout, utiliser immédiatement la méthode simple améliorée
                if (_niveau >= 5 && _moteurMinimax != null)
                {
                    try
                    {
                        var echiquier = _service.Echiquier;
                        var etatPartie = _service.EtatPartie;
                        
                        if (echiquier != null && etatPartie != null)
                        {
                            // Profondeur très réduite pour minimiser les timeouts
                            int profondeur = _niveau == 5 ? 2 : 2; // Même profondeur 2 pour les deux niveaux
                            Console.WriteLine($"[IA] Tentative Minimax avec profondeur {profondeur} pour couleur {_couleurIa} (timeout 3s)...");
                            
                            // Utiliser Task.Run avec timeout court pour éviter les blocages
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                            var minimaxTask = Task.Run(() => 
                            {
                                try
                                {
                                    return _moteurMinimax.MeilleurCoup(echiquier, etatPartie, _couleurIa, profondeur);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[IA] Exception dans Minimax: {ex.Message}");
                                    return (Coup?)null;
                                }
                            }, cts.Token);
                            
                            try
                            {
                                coup = await minimaxTask;
                                if (coup != null)
                                {
                                    Console.WriteLine($"[IA] Minimax a trouvé un coup: {(char)('a' + coup.ColonneDepart)}{8 - coup.LigneDepart} -> {(char)('a' + coup.ColonneArrivee)}{8 - coup.LigneArrivee}");
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine($"[IA] Minimax timeout après 3 secondes, fallback immédiat sur méthode simple");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[IA] Erreur lors de l'exécution de Minimax: {ex.Message}, fallback sur méthode simple");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[IA] Erreur globale Minimax: {ex.Message}, fallback sur méthode simple");
                    }
                }

                // Si Minimax n'a pas trouvé de coup, utiliser la méthode simple
                if (coup == null)
                {
                    var coups = _service.ObtenirTousCoupsPossibles();
                    Console.WriteLine($"[IA] Coups possibles trouvés: {coups?.Count ?? 0}");
                    
                    if (coups == null || coups.Count == 0)
                    {
                        Console.WriteLine($"[IA] Aucun coup possible !");
                        _enCours = false;
                        return;
                    }

                    Console.WriteLine($"[IA] Utilisation de ChoisirMeilleurCoup avec niveau {_niveau}...");
                    coup = _aiService.ChoisirMeilleurCoup(coups, _niveau);
                }

                Console.WriteLine($"[IA] Coup choisi: {(char)('a' + coup.ColonneDepart)}{8 - coup.LigneDepart} -> {(char)('a' + coup.ColonneArrivee)}{8 - coup.LigneArrivee}");
                
                // Utiliser les coordonnées individuelles au lieu de l'objet Coup complet
                // pour éviter les problèmes de propriétés manquantes
                bool succes = _service.JouerCoup(
                    coup.LigneDepart, 
                    coup.ColonneDepart, 
                    coup.LigneArrivee, 
                    coup.ColonneArrivee, 
                    coup.PiecePromotion
                );
                
                Console.WriteLine($"[IA] Coup joué avec succès: {succes}");

                _enCours = false;
            }
        }
    }
}
