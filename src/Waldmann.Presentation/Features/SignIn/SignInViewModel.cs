using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Waldmann.Core.Accounts;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation.Features.SignIn;

/// <summary>
/// The login of the seeded demo account, supplied by the composition root. As long as there is no
/// server, the sign-in page shows it; <see langword="null"/> hides the hint.
/// </summary>
public sealed record DemoAccountInfo(Credentials? Credentials);

/// <summary>
/// Replaces <c>Login.xaml.cs</c>, which navigated on without checking anything, named its entries
/// "entName"/"entsurName" and showed the password in plain text.
/// </summary>
public sealed partial class SignInViewModel(
    SignInUseCase signIn,
    DemoAccountInfo demoAccount,
    INavigationService navigation,
    IDialogService dialogs) : ViewModelBase(dialogs)
{
    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? password;

    public bool HasDemoAccount => demoAccount.Credentials is not null;

    public string DemoHint => demoAccount.Credentials is { } credentials
        ? DisplayFormat.Format(Strings.SignIn_PrototypeHint, credentials.Email, credentials.Password)
        : string.Empty;

    [RelayCommand]
    private Task SignInAsync() => RunBusyAsync(async () =>
    {
        var result = await signIn.ExecuteAsync(Email, Password);
        if (result.IsFailure)
        {
            await ReportFailureAsync(result.Error);
            return;
        }

        ValidationMessage = null;
        Password = null;
        await navigation.GoToAsync(Routes.Offers);
    });

    [RelayCommand]
    private void UseDemoAccount()
    {
        if (demoAccount.Credentials is { } credentials)
        {
            Email = credentials.Email;
            Password = credentials.Password;
        }
    }

    [RelayCommand]
    private Task OpenRegistrationAsync() => navigation.GoToAsync(Routes.Registration);
}
