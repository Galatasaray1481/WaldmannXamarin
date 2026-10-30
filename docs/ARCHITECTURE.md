# Architektur

## Schichtenmodell

Waldmann ist ein klickbarer Prototyp (Wildbret-Marktplatz und Jagd-Community) **ohne Server**. Die
Modernisierung bleibt bewusst bei Xamarin.Forms (ADR 1), trennt aber Fachlogik, Datenquelle,
Präsentationslogik und Oberfläche in eigene Projekte. Abhängigkeiten zeigen nur nach innen;
`tests/Waldmann.ArchitectureTests` prüft die Regeln bei jedem Build.

```
 ┌───────────────────────┐   ┌───────────────────────┐
 │ Waldmann.Android      │   │ Waldmann.iOS          │   klassische Xamarin-Kopfprojekte
 │ (Xamarin.Android, 33) │   │ (Xamarin.iOS)         │   (MainActivity / AppDelegate)
 └───────────┬───────────┘   └───────────┬───────────┘
             └─────────────┬─────────────┘
                           ▼
             ┌───────────────────────────┐
             │ Waldmann  (Xamarin.Forms) │  Pages (XAML), Shell, Shell-Dienste,
             │                           │  Bilder, CompositionRoot
             └──────┬─────────────┬──────┘
                    │             │ (nur CompositionRoot)
                    ▼             ▼
 ┌───────────────────────────┐  ┌──────────────────────────────┐
 │ Waldmann.Presentation     │  │ Waldmann.Infrastructure      │
 │  ViewModels, Texte,       │  │  In-Memory-Repositories      │
 │  Formatierung, Ports für  │  │  (Konten, Angebote, Chats),  │
 │  Navigation/Dialoge —     │  │  Passwort-Hashing            │
 │  OHNE Xamarin-Referenz    │  └──────────────┬───────────────┘
 └─────────────┬─────────────┘                 │
               └───────────────┬───────────────┘
                               ▼
                 ┌───────────────────────────┐
                 │ Waldmann.Core             │  Modelle, Eingaberegeln, Ports,
                 │  netstandard2.0, nur      │  Use Cases, Result/Error
                 │  Compile-Zeit-Polyfills   │
                 └───────────────────────────┘
```

Alle Bibliotheken zielen auf **netstandard2.0** (das höchste, was Xamarin.Android/iOS konsumieren),
die Testprojekte auf **net10.0**.

## Architekturentscheidungen (ADR)

### 1. Xamarin.Forms bleibt — mit bekannten Konsequenzen
**Kontext:** Vorgabe war, die App zu modernisieren, ohne nach .NET MAUI zu migrieren. Xamarin
(Xamarin.Forms, Xamarin.Android, Xamarin.iOS) hat am **1. Mai 2024 das Support-Ende** erreicht.
**Entscheidung:** Letzte Versionen (Xamarin.Forms 5.0.0.2662, Xamarin.Essentials 1.8.1,
Xamarin.Android 13.2 → Android 13 / API 33). Alles, was nicht Xamarin sein muss, liegt in
netstandard2.0-Bibliotheken ohne Xamarin-Referenz — eine spätere MAUI-Migration ersetzt nur das
UI-Projekt und die Köpfe.
**Konsequenzen (bewusst akzeptiert):**
- **Kein Play-Store-Upload:** Google Play verlangt für App-Updates ein höheres targetSdk als 33.
  Verteilung nur als APK (direkt/intern).
- **Toolchain:** Bauen nur mit Visual Studio 2022 (17.x) und Xamarin-Komponente; Visual Studio 2026
  enthält Xamarin nicht mehr.
- **CI:** Von den GitHub-Images hat nur noch `windows-2022` Visual Studio 2022 mit Xamarin
  (`windows-2025`, die VS-2026-Images und `macos-15` nicht). Wird `windows-2022` abgekündigt, braucht
  der Android-Job einen selbst gehosteten Windows-Runner mit VS 2022.
- **iOS:** Kein GitHub-Runner enthält Xamarin.iOS mehr; iOS wird nur lokal auf einem Mac (bzw.
  Windows mit gekoppeltem Mac) gebaut und ist in CI nicht abgedeckt.
- **Keine Sicherheitsupdates** für Xamarin selbst.

### 2. Präsentationsschicht ohne Xamarin
**Kontext:** In der Altversion lag alle Logik in Code-Behind-Dateien (`Login.xaml.cs` etc.) und war
nur mit laufender App prüfbar.
**Entscheidung:** ViewModels (CommunityToolkit.Mvvm) und alle Anzeigetexte liegen in
`Waldmann.Presentation` — ohne Xamarin-Referenz. Navigation und Dialoge sind Ports
(`INavigationService`, `IDialogService`), implementiert im UI-Projekt per Shell. Damit laufen alle
ViewModel-Tests auf reinem .NET, und das Projekt überlebt eine MAUI-Migration unverändert.

### 3. Shell statt MasterDetailPage + kopierter BottomBar
**Kontext:** `MainPage` war eine `MasterDetailPage` (in Xamarin.Forms 5 obsolet), dazu kam eine aus
einem Drittprojekt kopierte `BottomBarPage` samt Android-Renderer, der per Reflection in Interna von
Xamarin.Forms griff. Ihre fünf Tabs zeigten alle dieselbe Seite.
**Entscheidung:** Eine Xamarin.Forms-`Shell` mit Flyout. Die Shell wird in C# aufgebaut (`AppShell.cs`),
weil Xamarin.Forms-Shell Seiten sonst per parameterlosem Konstruktor erzeugt; so kommen alle Seiten
mit ihren ViewModels aus dem DI-Container. Die BottomBar entfällt ersatzlos.

### 4. In-Memory-Daten hinter Ports
**Kontext:** Es gibt keinen Server. Die Altversion hatte Daten als XAML-Literale (Angebote) bzw. im
Code-Behind (Chats) und ließ jede Anmeldung ohne Prüfung zu.
**Entscheidung:** `IAccountRepository`, `IOfferRepository`, `IChatRepository` (Core) mit
In-Memory-Implementierungen (Infrastructure), befüllt mit genau den Inhalten des Prototyps plus
einem Demo-Konto. Ein späterer Server ersetzt nur die Registrierungen in
`AddWaldmannInfrastructure()`. Auf der Anmeldeseite steht offen „Prototyp ohne Server“ samt
Demo-Zugang — es gibt keinen „echten“ Modus, vor dem man ihn verstecken müsste.

### 5. Funktionslose Menüpunkte werden ehrlich gekennzeichnet
**Kontext:** Mehrere Menüpunkte öffneten Kopien anderer Seiten: „Events“ und „Jäger finden“ eine leere
Bestellliste, „Jagdrecht“ die BurgerTasters-„Über uns“-Seite, „Begehungsscheine“ ein
Geld-Einzahlformular; Buttons luden ihre eigene Seite neu.
**Entscheidung:** Umfang „Struktur“: Bestehende Funktionen bleiben, es kommen keine neuen hinzu.
Menüpunkte ohne Funktion zeigen eine gemeinsame Seite „In Vorbereitung“ mit kurzer Beschreibung
(`UpcomingFeatureViewModel`). Profil zeigt die Kontodaten, Bearbeiten ist als „in Vorbereitung“
markiert; im Chat ist das Eingabefeld sichtbar deaktiviert.

### 6. Keine Klartext-Passwörter — auch nicht im Speicher
**Entscheidung:** Das In-Memory-Kontenregister speichert PBKDF2-HMAC-SHA256-Hashes (100 000
Iterationen, zufälliges Salt, zeitkonstanter Vergleich). netstandard2.0 bietet PBKDF2 nur mit SHA-1;
die SHA-256-Variante ist nach RFC 8018 implementiert und per Test gegen den Testvektor aus RFC 7914
abgesichert. Session und Domänenmodell enthalten kein Passwort; Eingabe-Records maskieren es in
`ToString()`.

### 7. C# 13 fest, netstandard2.0 mit Polyfills
**Kontext:** Das Android-Projekt wird von MSBuild aus Visual Studio 2022 gebaut, dessen Compiler C# 13
kennt; `dotnet build` (für Tests) nutzt den neueren Compiler des .NET-10-SDK.
**Entscheidung:** `LangVersion` 13 für alle Projekte — beide Wege kompilieren identisch. Records,
`init` und Nullable-Attribute kommen per PolySharp (nur Compile-Zeit). Folge: CommunityToolkit.Mvvm
wird im Feld-Stil (`[ObservableProperty] private string? email;`) genutzt, weil partielle
Eigenschaften erst ab C# 14 generiert werden.

### 8. Zentrale Paketverwaltung auch für die Xamarin-Köpfe
**Entscheidung:** `Directory.Packages.props` gilt für alle Projekte; die klassischen
Xamarin-Projektdateien importieren dafür `Microsoft.Common.props`. Die Android-Support-Bibliotheken
(neben AndroidX!) und das BottomNavigationBar-Paket sind entfernt. `Xamarin.AndroidX.RecyclerView` ist
explizit gepinnt, weil NuGet sonst eine ältere Version wählt, als `AndroidX.Preference` braucht
(NU1605).

### 9. Minimale Berechtigungen
**Entscheidung:** Die App verlangt keine Android-Berechtigungen mehr; INTERNET und
WRITE_EXTERNAL_STORAGE aus der Projektvorlage sind entfernt (ohne Server und Dateizugriff nicht nötig).
Ein Server-Backend bringt INTERNET wieder mit.

## Qualitäts-Gates

| Gate | Wo | Regel |
|---|---|---|
| Compiler + Analyzer | `Directory.Build.props`, `.editorconfig` | SDK-Projekte: `TreatWarningsAsErrors`, `latest-recommended`, Code-Style im Build |
| XAML | Xamarin.Forms XamlC | `XamlCompilation` + `x:DataType` auf jeder Seite (kompilierte Bindings) |
| Unit-Tests | `tests/Waldmann.{Core,Infrastructure,Presentation}.Tests` | Regeln, Use Cases, Repositories, ViewModels, Formatierung |
| Integrationstests | `tests/Waldmann.IntegrationTests` | echte Komposition (Infrastructure + Anwendung), User Journeys über die ViewModels |
| Architekturtests | `tests/Waldmann.ArchitectureTests` | Schichtregeln, Presentation ohne Xamarin, Pages ohne Infrastructure, keine globalen veränderlichen Felder |
| Coverage | `scripts/Test.ps1` | ≥ 90 % Zeilen über Core, Infrastructure, Presentation (aktuell ≈ 99 %) |
| Android-Build | CI-Job `android` (`windows-2022`) | Debug- und Release-APK |

## Bekannte Einschränkungen

- Siehe ADR 1 (Support-Ende, Play Store, Toolchain, iOS ohne CI).
- Daten leben nur bis zum Beenden der App (ADR 4).
- Das Android-Paket heißt weiterhin `com.companyname.waldmann` (Vorlagenname), ebenso
  `com.companyname.Waldmann` unter iOS — vor einer Verteilung an Endkunden festlegen; eine spätere
  Änderung macht die App auf Geräten zu einer neuen App.
