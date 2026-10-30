# Setup

## Voraussetzungen

| Komponente | Version | Wofür |
|---|---|---|
| Visual Studio 2022 (17.x) mit Workload „Mobile-Entwicklung mit .NET“ (Komponente Xamarin) | 17.14 getestet | Android-/iOS-Köpfe (MSBuild + Xamarin.Android 13.2 / Xamarin.iOS) |
| .NET SDK | 10.0.1xx (über `global.json`) | SDK-Projekte und Tests |
| JDK | 17 | Xamarin.Android-Build |
| Android SDK | Plattform `android-33`, Build-Tools 33+ | Xamarin.Android 13.2 (API 33) |
| Mac mit Xcode (nur iOS) | passend zu Xamarin.iOS 16.x | iOS-Build; in CI nicht verfügbar |

**Visual Studio 2026 genügt nicht** für die App: Es enthält Xamarin nicht mehr. Es kann aber die
Testprojekte bauen und ausführen. Umgekehrt meldet Visual Studio 2022 beim Bauen der Testprojekte die
Warnung NETSDK1233 („.NET 10 in VS 2022 nicht unterstützt“) — die Tests laufen trotzdem über
`dotnet test` bzw. `scripts/Test.ps1` (siehe ADR 1/7 in `ARCHITECTURE.md`).

`nuget.config` beschränkt die Paketquellen auf nuget.org; alle Versionen stehen in
`Directory.Packages.props`.

## Kommandos

```powershell
./scripts/Test.ps1                         # alle Tests + Coverage-Report + Schwelle (90 %)
dotnet test tests/Waldmann.Presentation.Tests

# Android (MSBuild aus Visual Studio 2022, z. B. in der "Developer PowerShell for VS 2022"):
msbuild src/Waldmann.Android/Waldmann.Android.csproj /restore /t:SignAndroidPackage /p:Configuration=Debug
#   -> src/Waldmann.Android/bin/Debug/com.companyname.waldmann-Signed.apk
msbuild src/Waldmann.Android/Waldmann.Android.csproj /t:Install /p:Configuration=Debug   # auf Emulator/Gerät
```

Fehlen JDK oder Android SDK im Standardpfad, `/p:JavaSdkDirectory=…` und
`/p:AndroidSdkDirectory=…` ergänzen. `Waldmann.sln` öffnet man in Visual Studio 2022; mit
`dotnet build Waldmann.sln` lässt sie sich nicht bauen (die Xamarin-Targets gibt es nur in
Visual Studio).

`scripts/Test.ps1` testet die Testprojekte einzeln und erzeugt `coverage/index.html`
(ReportGenerator als lokales Tool aus `.config/dotnet-tools.json`). Gemessen werden Core,
Infrastructure und Presentation (`coverage.runsettings`).

## Prototyp ohne Server

Alle Daten liegen im Speicher der App und sind nach einem Neustart zurückgesetzt. Die Anmeldeseite
zeigt den Demo-Zugang (`demo@waldmann.example` / `waldmann`); „Demo-Zugang eintragen“ füllt ihn ein.
Neue Konten lassen sich registrieren und gelten bis zum Beenden der App.

Ein Server ersetzt später nur die Registrierungen in
`src/Waldmann.Infrastructure/InfrastructureServiceCollectionExtensions.cs`; dann bitte auch die
INTERNET-Berechtigung wieder aufnehmen (ADR 9) und den Demo-Hinweis in `CompositionRoot.cs`
(`new DemoAccountInfo(null)`) abschalten.

## CI/CD (GitHub Actions)

Die Pipelines laufen **ausschließlich auf GitHub** — im Original-Repository auf github.com. Der Job
`guard` überspringt alles in Forks, auf GitHub-Enterprise-Servern und in lokalen Runnern wie `act`.

`.github/workflows/ci.yml` (Push auf `main`/`master`, Pull Requests, manuell):

| Job | Runner | Inhalt | Artefakte |
|---|---|---|---|
| `test` | `ubuntu-latest` | `scripts/Test.ps1 -Configuration Release`, Coverage ≥ 90 % | `coverage-report`, `test-results`; Coverage-Tabelle in der Job-Zusammenfassung |
| `android` | `windows-2022` | Debug- und Release-APK (Debug-Schlüssel), Versionscode = Laufnummer | `waldmann-Debug-apk`, `waldmann-Release-apk` |

`.github/workflows/release.yml` baut bei einem Tag `v*` (oder manuell mit Versionsnummer) ein
signiertes Release-APK und legt einen **Entwurf** eines GitHub-Release an. Repository-Secrets:

| Secret | Inhalt |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | Keystore, base64 (`[Convert]::ToBase64String([IO.File]::ReadAllBytes('release.keystore'))`) |
| `ANDROID_KEYSTORE_PASSWORD` | Passwort des Keystores |
| `ANDROID_KEY_ALIAS` | Alias des Schlüssels |
| `ANDROID_KEY_PASSWORD` | Passwort des Schlüssels |

Fehlt `ANDROID_KEYSTORE_BASE64`, überspringt sich der Release-Job. Versionsname = Tag ohne `v`,
Versionscode = Laufnummer; beides schreibt `scripts/Set-AndroidVersion.ps1` ins Android-Manifest
(klassisches Xamarin hat dafür keine MSBuild-Eigenschaften). Das APK ist für direkte bzw. interne
Verteilung gedacht — der Play Store nimmt targetSdk 33 nicht mehr an (ADR 1).

### Wenn `windows-2022` abgekündigt wird

Dann gibt es kein GitHub-Image mit Visual Studio 2022 + Xamarin mehr. Optionen: ein selbst
gehosteter Windows-Runner mit Visual Studio 2022 (in `ci.yml`/`release.yml` `runs-on` anpassen und
den Runner per Label auf dieses Repository beschränken) — oder die Migration nach .NET MAUI, die
durch die Schichtung auf das UI-Projekt und die Köpfe begrenzt ist (ADR 1/2).

### iOS

Kein GitHub-Runner enthält mehr Xamarin.iOS. Das iOS-Projekt wird nur lokal (Mac bzw. Windows mit
gekoppeltem Mac) gebaut; die Aktualisierung bei der Modernisierung beschränkt sich auf zentrale
Paketversionen und iOS 12 als Mindestversion und ist mangels Mac **ungeprüft**.
