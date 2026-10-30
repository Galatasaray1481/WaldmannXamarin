using Waldmann.Presentation.Features.Registration;

namespace Waldmann.Views;

public partial class RegistrationPage : ViewModelPage
{
    public RegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
