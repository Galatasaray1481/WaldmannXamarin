using Waldmann.Core.Accounts;
using Waldmann.Core.Common;

namespace Waldmann.Core.Chats;

/// <summary>One chat message. Replaces the legacy <c>UserMsgInfo</c>, which mixed first name and role into one name field.</summary>
public sealed record ChatMessage(string SenderName, string SenderDescription, string Content);

/// <summary>Port for the chat history.</summary>
public interface IChatRepository
{
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(UserId userId, CancellationToken cancellationToken);
}

/// <summary>The chat history of the signed-in user.</summary>
public sealed class GetChatMessagesUseCase(IChatRepository chats, IUserSession session)
{
    public async Task<Result<IReadOnlyList<ChatMessage>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (session.CurrentAccount is not { } account)
        {
            return Error.Of(ErrorKind.NotSignedIn);
        }

        var messages = await chats.GetMessagesAsync(account.Id, cancellationToken).ConfigureAwait(false);
        return Result<IReadOnlyList<ChatMessage>>.Success(messages);
    }
}
