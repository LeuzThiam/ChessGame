# ChessGame

Projet d'application d'échecs en .NET, structuré selon les principes de la Clean Architecture, avec une interface utilisateur .NET MAUI.

## Description du projet

`ChessGame` est un projet d'apprentissage / consolidation autour du développement d'un jeu d'échecs en C#.
L'objectif est de séparer clairement les responsabilités :

- `Core` : logique métier et règles du jeu
- `Infrastructure` : persistance et implémentations techniques
- `SharedKernel` : composants transversaux partagés
- `MAUI` : interface utilisateur multiplateforme (focus local Windows pour le lancement)

## Structure des dossiers

```text
ChessGame/
├── ChessGame.Core/               # Domaine / application (métier)
├── ChessGame.Infrastructure/     # Accès aux données, EF Core, implémentations techniques
├── ChessGame.SharedKernel/       # Types et utilitaires partagés
├── ChessGame.TestsUnitaires/     # Tests unitaires
├── ChessGame.TestsIntegration/   # Tests d'intégration
├── ChessGameMAUI/                # Application UI .NET MAUI
└── ChessGame.sln                 # Solution Visual Studio
```

## Technologies utilisées

- .NET 8 (Core / Infrastructure / SharedKernel / Tests)
- .NET MAUI (projet UI `ChessGameMAUI`)
- C#
- Entity Framework Core
- xUnit (tests)
- GitHub Actions (CI)

## Prérequis

- SDK .NET 8 installé
- Workload .NET MAUI installé pour lancer l'application UI :

```powershell
dotnet workload install maui
```

- Visual Studio 2022 (recommandé) avec composants `.NET MAUI` (optionnel mais pratique)

## Instructions pour lancer le projet

### 1. Restaurer les dépendances

```powershell
dotnet restore ChessGame.sln
```

### 2. Compiler la solution

```powershell
dotnet build ChessGame.sln
```

### 3. Lancer l'application MAUI (Windows)

Depuis le dossier `ChessGameMAUI` :

```powershell
dotnet build -t:Run -f net9.0-windows10.0.19041.0 -maxcpucount:1
```

> Le projet MAUI cible notamment Windows (`net9.0-windows10.0.19041.0`). Assurez-vous d'avoir les workloads et SDK nécessaires installés localement.

### 4. Exécuter les tests

```powershell
dotnet test ChessGame.TestsUnitaires/ChessGame.TestsUnitaires.csproj
dotnet test ChessGame.TestsIntegration/ChessGame.TestsIntegration.csproj
```

## CI/CD

Le dépôt inclut un workflow GitHub Actions (`.github/workflows/ci.yml`) qui :

- restaure les dépendances
- compile les projets non-MAUI
- exécute les tests unitaires et d'intégration

### Pourquoi le projet MAUI est exclu de la CI Ubuntu ?

Les runners Ubuntu GitHub Actions ne disposent pas par défaut de l'environnement MAUI complet (workloads, SDKs plateforme). Pour garder une CI stable, le workflow compile et teste les couches métier/infrastructure/tests uniquement.

## Roadmap

- [ ] Finaliser les règles métier (mouvements spéciaux, roque, prise en passant, promotion)
- [ ] Renforcer la validation des coups illégaux
- [ ] Améliorer la persistance des parties (historique / reprise)
- [ ] Ajouter davantage de tests unitaires
- [ ] Ajouter des tests d'intégration EF Core
- [ ] Mettre en place analyse de qualité (format, analyzers, couverture)
- [ ] Ajouter CD (packaging / artefacts) si publication prévue

## Licence

Ce projet est distribué sous licence MIT.

Voir le fichier `LICENSE` (à ajouter) pour le texte complet.

