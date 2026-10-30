using Waldmann.Presentation.Features.SignIn;

namespace Waldmann.Views;

public partial class SignInPage : ViewModelPage
{
    public SignInPage(SignInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
