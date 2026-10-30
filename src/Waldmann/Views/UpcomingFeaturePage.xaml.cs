using Waldmann.Presentation.Features.Upcoming;

namespace Waldmann.Views;

public partial class UpcomingFeaturePage : ViewModelPage
{
    public UpcomingFeaturePage(UpcomingFeatureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
