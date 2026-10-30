# Waldmann — App (Prototyp)

Prototyp einer App für Wildbret-Käufer und Jäger: Wildbretkauf, Profil, Chats sowie Menüpunkte für
Begehungsscheine, Events/Schießstände, Jägersuche, Jagdrecht und Warenkorb (in Vorbereitung).
Xamarin.Forms 5 für Android und iOS, **ohne Server** — alle Daten liegen im Speicher der App.

> **Xamarin hat das Support-Ende erreicht (1. Mai 2024).** Die App bleibt bewusst bei Xamarin; was das
> für Play Store, Toolchain und CI bedeutet, steht in ADR 1 in [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Architektur

`Core` (Modelle, Regeln, Use Cases) ← `Infrastructure` (In-Memory-Daten) und `Presentation`
(ViewModels **ohne** Xamarin-Referenz) ← `Waldmann` (Xamarin.Forms-UI, Shell) ← Android-/iOS-Köpfe.
Begründungen: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md). Zuordnung alt → neu:
[`docs/MIGRATION.md`](docs/MIGRATION.md).

## Loslegen

Voraussetzungen, Kommandos, CI-Secrets: [`docs/SETUP.md`](docs/SETUP.md).

```powershell
./scripts/Test.ps1                                                     # Tests + Coverage-Schwelle
msbuild src/Waldmann.Android/Waldmann.Android.csproj /restore /t:Install  # Visual Studio 2022
```

Demo-Zugang: `demo@waldmann.example` / `waldmann`.

## Qualität

| Gate | Kommando | Bericht |
|---|---|---|
| Compiler, Analyzer, Code-Style (Warnungen = Fehler) | `dotnet build` | Konsole |
| Unit-, Integrations-, Architekturtests | `./scripts/Test.ps1` | `TestResults/**/*.trx` |
| Coverage (min. 90 % Zeilen) | `./scripts/Test.ps1` | `coverage/index.html` |
| Android-APK (Debug + Release) | CI-Job `android` | Workflow-Artefakte |

## Projektübersicht

```
src/Waldmann.Core            Modelle, Eingaberegeln, Ports, Use Cases (netstandard2.0)
src/Waldmann.Infrastructure  In-Memory-Repositories, Passwort-Hashing (netstandard2.0)
src/Waldmann.Presentation    ViewModels, Texte, Formatierung — ohne Xamarin (netstandard2.0)
src/Waldmann                 Xamarin.Forms: Pages, Shell, CompositionRoot, Bilder
src/Waldmann.Android         Xamarin.Android-Kopf (API 33)
src/Waldmann.iOS             Xamarin.iOS-Kopf (nur auf Mac baubar)
tests/                       Unit-, Integrations-, Architekturtests (net10.0)
.github/workflows            CI (Tests, Coverage, APKs) und Release
```
