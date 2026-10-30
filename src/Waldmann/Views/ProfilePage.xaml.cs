using Waldmann.Presentation.Features.Profile;

namespace Waldmann.Views;

public partial class ProfilePage : ViewModelPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
