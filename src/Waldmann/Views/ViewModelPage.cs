using Waldmann.Presentation.Navigation;
using Xamarin.Forms;

namespace Waldmann.Views;

/// <summary>Base page: forwards the appearing event to the ViewModel, so ViewModels never need a Xamarin type.</summary>
public abstract class ViewModelPage : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is IAppearingAware aware)
        {
            // async void is unavoidable for this override; ViewModels report their own failures.
            await aware.OnAppearingAsync();
        }
    }
}
