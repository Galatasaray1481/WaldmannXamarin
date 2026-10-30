# Migration: Xamarin-Prototyp → strukturierte Xamarin-App

Diese Tabellen dokumentieren, wohin jede Datei der Altversion gewandert ist. Der alte Code existiert
nicht mehr im Baum; eine Sicherung liegt außerhalb des Repositorys
(`Waldmann-Xamarin-legacy-backup.zip` neben dem Repository-Ordner). Maßgeblich ist die Git-Historie.

## Projekte

| Alt | Neu |
|---|---|
| `Waldmann/` (netstandard2.0, Xamarin.Forms 5.0.0.2012, alles in einem Projekt) | `src/Waldmann.Core`, `src/Waldmann.Infrastructure`, `src/Waldmann.Presentation`, `src/Waldmann` (nur noch UI) |
| `Waldmann.Android/` (API 30, Support-Libraries **und** AndroidX, BottomNavigationBar) | `src/Waldmann.Android/` (API 33, nur AndroidX über Xamarin.Forms, keine Berechtigungen) |
| `Waldmann.iOS/` (iOS 8) | `src/Waldmann.iOS/` (iOS 12, zentrale Paketversionen) |
| `Waldmann.sln` | `Waldmann.sln` (mit Testprojekten) |
| — | `tests/*` (84 Tests), `scripts/`, `.github/workflows/`, `docs/` |

## Seiten

| Alt (`Waldmann/…`) | Neu | Verhalten alt → neu |
|---|---|---|
| `App.xaml(.cs)` (`new NavigationPage(new MainPage())` — Start **ohne** Login) | `App.xaml(.cs)`, `AppShell.cs`, `CompositionRoot.cs` | Start auf der Anmeldeseite |
| `Login.xaml(.cs)` | `Views/SignInPage` + `Presentation/Features/SignIn/SignInViewModel` | vorher: jede Eingabe führte weiter, Passwort im Klartext sichtbar; jetzt: Prüfung gegen das Kontenregister, Passwortfeld maskiert, Demo-Zugang |
| `Register.xaml(.cs)` | `Views/RegistrationPage` + `RegistrationViewModel` | vorher: Button navigierte nur zurück, Rollen-Auswahl leer, Info-Icon ohne Bild; jetzt: Validierung, Rollen „Jäger/in“/„Wildbretkäufer/in“, Konto wird angelegt |
| `MainPage.xaml(.cs)` (MasterDetailPage), `MenuItems/MasterPageItem.cs` | `AppShell.cs` (Flyout) | gleiche acht Menüpunkte, zusätzlich „Abmelden“ |
| `MainPageMaster.*`, `MainPageDetail.*`, `MainPageMenuItem.cs` | entfällt | ungenutzte Vorlagenreste |
| `BottomNavBar.xaml(.cs)`, `BottomBar/*`, `Waldmann.Android/BottomBar/*` | entfällt | fünf Tabs zeigten alle `MenuCard`; Renderer griff per Reflection in Xamarin-Interna |
| `Views/MenuCard.xaml(.cs)` (Wildbretkauf) | `Views/OffersPage` + `OffersViewModel`; Daten in `InMemoryOfferRepository` | gleiche sechs Angebote und Preise; Herz-Icons und Filter-Icon ohne Funktion entfernt; Sterne als Text statt Bild; sortiert nach Tierart und Preis |
| `Views/PayIn.xaml(.cs)` („Begehungsscheine kaufen“) | `UpcomingFeaturePage` (In Vorbereitung) | war ein kopiertes Geld-Einzahlformular ohne Funktion |
| `Views/OrderOverview.xaml(.cs)` („Events u. Schießstände“, „Jäger finden“) | je eine `UpcomingFeaturePage` | war eine leere Bestellliste |
| `Views/EditProfile.xaml(.cs)` | `Views/ProfilePage` + `ProfileViewModel` | zeigt Name, E-Mail, Rolle; Bearbeiten/Löschen als „in Vorbereitung“ (Buttons luden vorher nur die Seite neu) |
| `Views/PrivateChat.xaml(.cs)`, `UserMsgInfo.cs` | `Views/ChatsPage` + `ChatsViewModel`; `Core/Chats/ChatMessage`, `InMemoryChatRepository` | gleiche zwei Nachrichten; Senden sichtbar deaktiviert (hatte keine Funktion) |
| `Views/AboutUs.xaml(.cs)` („Jagdrecht einsehen“) | `UpcomingFeaturePage` | war die BurgerTasters-„Über uns“-Seite mit leerem Text |
| `Views/Cart.xaml(.cs)` („Warenkorb“) | `UpcomingFeaturePage` | Buttons ohne Funktion |

## Altlasten aus BurgerTasters

Der Prototyp war aus BurgerTasters kopiert. Entfernt:

| Fundstelle | Problem |
|---|---|
| `AboutUs`, `EditProfile`, `PayIn`: `ImageSource.FromResource("BurgerTasters.Images…")` | Bilder existierten nicht → leere Flächen |
| `MenuCard`: `Waldmann.Images.filtericon.png` | Datei hieß `filtericon_cut.png` |
| `MenuCard.xaml.cs`: `houseTasteTapped`, `porkHamTasteTapped` … | Handler mit auskommentiertem Inhalt und `throw ex;` |
| `public static String Cash` in fünf Seiten, `xCash`-Label in der Titelleiste | Guthaben gibt es in Waldmann nicht |
| `WebClient request = new WebClient()` in vier Seiten | nie benutzt |

## Statische Felder → Dienste

| Alt | Neu |
|---|---|
| `Login.userID`, `Login.Username`, `Login.Password` (`public static`) | `IUserSession` (Singleton) mit `UserAccount`, ohne Passwort |
| `new SomePage()` / `Navigation.PushAsync(new SomePage())` in allen Seiten | `INavigationService` + `Routes`, Seiten aus dem DI-Container |
| 24 × `ImageSource.FromResource("…")` im Code-Behind | `EmbeddedImages`, `{local:EmbeddedImage …}`, `EmbeddedImageConverter` |

## Bilder

`Waldmann/Images/*` (14 Dateien) → `src/Waldmann/Images/*` (7 Dateien, neu kodiert, sprechende
Namen: `offer_*.png`, `waldmann_logo.png`). Entfallen: `fourStars.png`, `redcircle.png`,
`filtericon_cut.png` (durch Text bzw. Wegfall ersetzt) und vier nie verwendete Bilder. Die
Android-Drawables `ic_*.png` (BottomBar) entfallen. Ebenso `itemIcon1–7.png`: Sie wurden im alten
Menü angezeigt, aber nur unter Android (iOS hatte keine); die Shell-Menüpunkte kommen
plattformgleich ohne Icons aus. Sollen sie zurück, genügt `FlyoutIcon` je Eintrag in `AppShell.cs`
plus Bilder für beide Plattformen.
