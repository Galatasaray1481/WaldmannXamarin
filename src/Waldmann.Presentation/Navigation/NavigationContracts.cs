namespace Waldmann.Presentation.Navigation;

/// <summary>Every route of the app. Absolute routes (//) replace the navigation stack.</summary>
public static class Routes
{
    public const string SignIn = "//signin";
    public const string Offers = "//offers";
    public const string Permits = "//permits";
    public const string Events = "//events";
    public const string Profile = "//profile";
    public const string Hunters = "//hunters";
    public const string Chats = "//chats";
    public const string HuntingLaw = "//law";
    public const string Cart = "//cart";

    public const string Registration = "registration";
}

/// <summary>
/// Navigation as seen by ViewModels. Replaces <c>Navigation.PushAsync(new SomePage())</c> in code-behind,
/// which the legacy pages also used to "reload" themselves after a button tap.
/// </summary>
public interface INavigationService
{
    Task GoToAsync(string route);

    Task GoBackAsync();
}

/// <summary>Implemented by ViewModels that load data when their page becomes visible.</summary>
public interface IAppearingAware
{
    Task OnAppearingAsync();
}

/// <summary>Modal messages as seen by ViewModels.</summary>
public interface IDialogService
{
    Task ShowMessageAsync(string title, string message);
}
