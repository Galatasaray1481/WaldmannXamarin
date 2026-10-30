using System.Collections.ObjectModel;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation.Features.Chats;

/// <summary>One line of the chat list.</summary>
public sealed record ChatLine(string Sender, string Content)
{
    public static ChatLine From(ChatMessage message)
    {
        Guard.NotNull(message);
        return new ChatLine(DisplayFormat.Format(Strings.Chats_Sender, message.SenderName, message.SenderDescription), message.Content);
    }
}

/// <summary>
/// Replaces <c>Views/PrivateChat.xaml(.cs)</c>. The messages come from the chat port instead of being
/// constructed in the page; the "Send!" button had no function and is shown as in preparation.
/// </summary>
public sealed partial class ChatsViewModel(GetChatMessagesUseCase getMessages, INavigationService navigation, IDialogService dialogs)
    : ViewModelBase(dialogs), IAppearingAware
{
    public ObservableCollection<ChatLine> Messages { get; } = [];

    public Task OnAppearingAsync() => RunBusyAsync(async () =>
    {
        var result = await getMessages.ExecuteAsync();
        if (result.IsFailure)
        {
            await ReportFailureAsync(result.Error);
            await navigation.GoToAsync(Routes.SignIn);
            return;
        }

        Messages.Clear();
        foreach (var message in result.Value)
        {
            Messages.Add(ChatLine.From(message));
        }
    });
}
