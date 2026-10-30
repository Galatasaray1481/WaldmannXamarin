using Microsoft.Extensions.DependencyInjection;
using Waldmann.Presentation.Features;
using Waldmann.Presentation.Features.Upcoming;
using Waldmann.Presentation.Navigation;
using Waldmann.Presentation.Resources;
using Waldmann.Views;
using Xamarin.Forms;

namespace Waldmann;

/// <summary>
/// Flyout navigation. Replaces <c>MainPage</c> (MasterDetailPage, obsolete in Xamarin.Forms 5), its
/// unused template leftovers <c>MainPageMaster</c>/<c>MainPageDetail</c>, and the copied third-party
/// <c>BottomBarPage</c> with its reflection-based Android renderer, whose five tabs all showed the same page.
/// </summary>
/// <remarks>
/// Built in code rather than XAML: Xamarin.Forms Shell creates pages from DataTemplates with
/// parameterless constructors, so the templates here resolve the pages from the container instead.
/// </remarks>
public sealed class AppShell : Shell
{
    public AppShell(IServiceProvider services, AppShellViewModel viewModel)
    {
        FlyoutHeader = CreateHeader();

        var signIn = Content("signin", Strings.SignIn_Title, () => services.GetRequiredService<SignInPage>());
        SetFlyoutItemIsVisible(signIn, false);
        Items.Add(signIn);

        Items.Add(Content("offers", Strings.Shell_Offers, () => services.GetRequiredService<OffersPage>()));
        Items.Add(Content("permits", Strings.Shell_Permits, () => Upcoming(UpcomingFeature.HuntingPermits)));
        Items.Add(Content("events", Strings.Shell_Events, () => Upcoming(UpcomingFeature.EventsAndShootingRanges)));
        Items.Add(Content("profile", Strings.Shell_Profile, () => services.GetRequiredService<ProfilePage>()));
        Items.Add(Content("hunters", Strings.Shell_Hunters, () => Upcoming(UpcomingFeature.FindHunters)));
        Items.Add(Content("chats", Strings.Shell_Chats, () => services.GetRequiredService<ChatsPage>()));
        Items.Add(Content("law", Strings.Shell_HuntingLaw, () => Upcoming(UpcomingFeature.HuntingLaw)));
        Items.Add(Content("cart", Strings.Shell_Cart, () => Upcoming(UpcomingFeature.Cart)));
        Items.Add(new MenuItem { Text = Strings.Shell_SignOut, Command = viewModel.SignOutCommand });

        Routing.RegisterRoute(Routes.Registration, new ServiceRouteFactory<RegistrationPage>(services));
    }

    private static ShellItem Content(string route, string title, Func<Page> createPage)
    {
        ShellItem item = new ShellContent { Route = route, Title = title, ContentTemplate = new DataTemplate(createPage) };
        return item;
    }

    private static UpcomingFeaturePage Upcoming(UpcomingFeature feature) => new(new UpcomingFeatureViewModel(feature));

    private static StackLayout CreateHeader() =>
        new StackLayout
        {
            BackgroundColor = (Color)Application.Current.Resources["Primary"],
            Padding = new Thickness(20, 32, 20, 20),
            Children =
            {
                new Image { Source = EmbeddedImages.Get("waldmann_logo"), HeightRequest = 72, HorizontalOptions = LayoutOptions.Start },
                new Label { Text = Strings.AppName, TextColor = Color.White, FontSize = 22, FontAttributes = FontAttributes.Bold },
            },
        };

    private sealed class ServiceRouteFactory<TPage>(IServiceProvider services) : RouteFactory
        where TPage : Page
    {
        public override Element GetOrCreate() => services.GetRequiredService<TPage>();
    }
}
