using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Waldmann.Core.Accounts;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation.Features.Registration;

/// <summary>An entry of the role picker.</summary>
public sealed record RoleOption(UserRole Role, string Label)
{
    public override string ToString() => Label;
}

/// <summary>
/// Replaces <c>Register.xaml.cs</c>, which only navigated back. Its role picker had no entries and the
/// password info icon no image.
/// </summary>
public sealed partial class RegistrationViewModel(
    RegisterUseCase register,
    INavigationService navigation,
    IDialogService dialogs) : ViewModelBase(dialogs)
{
    public IReadOnlyList<RoleOption> Roles { get; } =
        [.. Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(role => new RoleOption(role, DisplayFormat.Role(role)))];

    public string PasswordHint { get; } = DisplayFormat.Format(Strings.Registration_PasswordHint, Core.Validation.AccountRules.MinimumPasswordLength);

    [ObservableProperty]
    private string? firstName;

    [ObservableProperty]
    private string? lastName;

    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private string? passwordConfirmation;

    [ObservableProperty]
    private RoleOption? selectedRole;

    [RelayCommand]
    private Task RegisterAsync() => RunBusyAsync(async () =>
    {
        var input = new RegistrationInput(FirstName, LastName, Email, Password, PasswordConfirmation, SelectedRole?.Role);
        var result = await register.ExecuteAsync(input);
        if (result.IsFailure)
        {
            await ReportFailureAsync(result.Error);
            return;
        }

        ValidationMessage = null;
        await Dialogs.ShowMessageAsync(Strings.Registration_SuccessTitle, Strings.Registration_SuccessMessage);
        await navigation.GoBackAsync();
    });
}
