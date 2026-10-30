using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;
using Xamarin.Forms;

namespace Waldmann.Services;

/// <summary><see cref="INavigationService"/> on top of Xamarin.Forms Shell routing.</summary>
internal sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoBackAsync() => Shell.Current.GoToAsync("..");
}

/// <summary><see cref="IDialogService"/> using the alerts of the current Shell page.</summary>
internal sealed class ShellDialogService : IDialogService
{
    public Task ShowMessageAsync(string title, string message) =>
        Shell.Current.DisplayAlert(title, message, Strings.Common_Ok);
}
