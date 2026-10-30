using Waldmann.Core.Accounts;
using Waldmann.Core.Common;

namespace Waldmann.Infrastructure.InMemory;

/// <summary>
/// Accounts in memory, seeded with one demo account. The legacy login accepted anything; with this
/// store, sign-in and registration follow real rules while there is still no server.
/// </summary>
internal sealed class InMemoryAccountRepository : IAccountRepository
{
    public static readonly Credentials DemoCredentials = new("demo@waldmann.example", "waldmann");

    private readonly object gate = new();
    private readonly Dictionary<string, StoredAccount> accountsByEmail = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryAccountRepository() =>
        Add(new UserAccount(UserId.New(), "Max", "Mustermann", DemoCredentials.Email, UserRole.Hunter), DemoCredentials.Password);

    public Task<Result<UserAccount>> SignInAsync(Credentials credentials, CancellationToken cancellationToken)
    {
        Guard.NotNull(credentials);
        cancellationToken.ThrowIfCancellationRequested();

        StoredAccount? stored;
        lock (gate)
        {
            accountsByEmail.TryGetValue(credentials.Email, out stored);
        }

        var result = stored is not null && PasswordHasher.Verify(credentials.Password, stored.PasswordHash)
            ? Result<UserAccount>.Success(stored.Account)
            : Result<UserAccount>.Failure(Error.Of(ErrorKind.InvalidCredentials));
        return Task.FromResult(result);
    }

    public Task<Result<UserAccount>> RegisterAsync(Registration registration, CancellationToken cancellationToken)
    {
        Guard.NotNull(registration);
        cancellationToken.ThrowIfCancellationRequested();

        var account = new UserAccount(UserId.New(), registration.FirstName, registration.LastName, registration.Email, registration.Role);
        lock (gate)
        {
            if (accountsByEmail.ContainsKey(registration.Email))
            {
                return Task.FromResult(Result<UserAccount>.Failure(Error.Of(ErrorKind.AccountAlreadyExists)));
            }

            Add(account, registration.Password);
        }

        return Task.FromResult(Result<UserAccount>.Success(account));
    }

    private void Add(UserAccount account, string password) =>
        accountsByEmail[account.Email] = new StoredAccount(account, PasswordHasher.Hash(password));

    private sealed record StoredAccount(UserAccount Account, string PasswordHash);
}
