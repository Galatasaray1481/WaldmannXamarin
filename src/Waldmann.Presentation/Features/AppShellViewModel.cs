using CommunityToolkit.Mvvm.Input;
using Waldmann.Core.Accounts;
using Waldmann.Presentation.Navigation;

namespace Waldmann.Presentation.Features;

/// <summary>Sign-out entry of the flyout (the legacy app had no way to sign out).</summary>
public sealed partial class AppShellViewModel(IUserSession session, INavigationService navigation)
{
    [RelayCommand]
    private Task SignOutAsync()
    {
        session.End();
        return navigation.GoToAsync(Routes.SignIn);
    }
}
