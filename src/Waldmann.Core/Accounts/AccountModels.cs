using Waldmann.Core.Common;

namespace Waldmann.Core.Accounts;

/// <summary>Identifies an account.</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}

/// <summary>The two kinds of users the prototype distinguishes (legacy registration picker "Rolle").</summary>
public enum UserRole
{
    Hunter,
    GameMeatBuyer,
}

/// <summary>A signed-in user. Holds no password.</summary>
public sealed record UserAccount(UserId Id, string FirstName, string LastName, string Email, UserRole Role)
{
    public string DisplayName => $"{FirstName} {LastName}";
}

/// <summary>Short-lived login input; never stored in the session.</summary>
public sealed record Credentials(string Email, string Password)
{
    public override string ToString() => $"Credentials {{ Email = {Email}, Password = *** }}";
}

/// <summary>Input for creating an account.</summary>
public sealed record Registration(string FirstName, string LastName, string Email, string Password, UserRole Role)
{
    public override string ToString() => $"Registration {{ Email = {Email}, Role = {Role}, Password = *** }}";
}

/// <summary>Port for account storage. Today an in-memory implementation; a server later.</summary>
public interface IAccountRepository
{
    /// <returns><see cref="ErrorKind.InvalidCredentials"/> for an unknown address or wrong password.</returns>
    Task<Result<UserAccount>> SignInAsync(Credentials credentials, CancellationToken cancellationToken);

    /// <returns><see cref="ErrorKind.AccountAlreadyExists"/> if the e-mail address is taken.</returns>
    Task<Result<UserAccount>> RegisterAsync(Registration registration, CancellationToken cancellationToken);
}
