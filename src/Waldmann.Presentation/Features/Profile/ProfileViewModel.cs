using CommunityToolkit.Mvvm.ComponentModel;
using Waldmann.Core.Accounts;
using Waldmann.Presentation.Navigation;

namespace Waldmann.Presentation.Features.Profile;

/// <summary>
/// Shows the signed-in account. The legacy <c>EditProfile</c> page had input fields and "Ändern"/"Konto
/// löschen" buttons without function (they reopened the page); editing is marked as in preparation.
/// </summary>
public sealed partial class ProfileViewModel(IUserSession session) : ObservableObject, IAppearingAware
{
    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string roleText = string.Empty;

    public Task OnAppearingAsync()
    {
        if (session.CurrentAccount is { } account)
        {
            DisplayName = account.DisplayName;
            Email = account.Email;
            RoleText = DisplayFormat.Role(account.Role);
        }

        return Task.CompletedTask;
    }
}
