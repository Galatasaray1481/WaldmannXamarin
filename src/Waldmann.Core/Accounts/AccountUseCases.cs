using Waldmann.Core.Common;
using Waldmann.Core.Validation;

namespace Waldmann.Core.Accounts;

/// <summary>Validates the input, signs in and starts the session.</summary>
public sealed class SignInUseCase(IAccountRepository accounts, IUserSession session)
{
    public async Task<Result<UserAccount>> ExecuteAsync(string? email, string? password, CancellationToken cancellationToken = default)
    {
        var issues = AccountRules.ValidateCredentials(email, password);
        if (issues.Count > 0)
        {
            return Error.Validation(issues);
        }

        var result = await accounts.SignInAsync(new Credentials(email!.Trim(), password!), cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            session.Start(result.Value);
        }

        return result;
    }
}

/// <summary>Raw form input for <see cref="RegisterUseCase"/>.</summary>
public sealed record RegistrationInput(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password,
    string? PasswordConfirmation,
    UserRole? Role)
{
    public override string ToString() => $"RegistrationInput {{ Email = {Email}, Role = {Role}, Password = *** }}";
}

/// <summary>Validates the input and creates the account. The user signs in afterwards, as before.</summary>
public sealed class RegisterUseCase(IAccountRepository accounts)
{
    public async Task<Result> ExecuteAsync(RegistrationInput input, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(input);
        var issues = AccountRules.ValidateRegistration(
            input.FirstName,
            input.LastName,
            input.Email,
            input.Password,
            input.PasswordConfirmation,
            input.Role.HasValue);
        if (issues.Count > 0)
        {
            return Error.Validation(issues);
        }

        var registration = new Registration(
            input.FirstName!.Trim(),
            input.LastName!.Trim(),
            input.Email!.Trim(),
            input.Password!,
            input.Role!.Value);

        return await accounts.RegisterAsync(registration, cancellationToken).ConfigureAwait(false);
    }
}
