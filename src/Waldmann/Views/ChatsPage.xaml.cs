using Waldmann.Presentation.Features.Chats;

namespace Waldmann.Views;

public partial class ChatsPage : ViewModelPage
{
    public ChatsPage(ChatsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
