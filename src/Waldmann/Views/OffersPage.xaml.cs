using Waldmann.Presentation.Features.Offers;

namespace Waldmann.Views;

public partial class OffersPage : ViewModelPage
{
    public OffersPage(OffersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
