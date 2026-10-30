using Waldmann.Core.Common;

namespace Waldmann.Core.Accounts;

/// <summary>
/// The signed-in user. Replaces the legacy <c>public static int userID</c> / <c>Username</c> /
/// <c>Password</c> fields on the login page.
/// </summary>
public interface IUserSession
{
    UserAccount? CurrentAccount { get; }

    bool IsSignedIn { get; }

    void Start(UserAccount account);

    void End();
}

/// <inheritdoc />
public sealed class UserSession : IUserSession
{
    private readonly object gate = new();
    private UserAccount? currentAccount;

    public UserAccount? CurrentAccount
    {
        get
        {
            lock (gate)
            {
                return currentAccount;
            }
        }
    }

    public bool IsSignedIn => CurrentAccount is not null;

    public void Start(UserAccount account)
    {
        Guard.NotNull(account);
        lock (gate)
        {
            currentAccount = account;
        }
    }

    public void End()
    {
        lock (gate)
        {
            currentAccount = null;
        }
    }
}
