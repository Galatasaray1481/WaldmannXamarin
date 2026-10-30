using Microsoft.Extensions.DependencyInjection;
using Waldmann.Infrastructure;
using Waldmann.Presentation;
using Waldmann.Presentation.Features.SignIn;
using Waldmann.Presentation.Navigation;
using Waldmann.Services;
using Waldmann.Views;

namespace Waldmann;

/// <summary>The only place that knows every layer. Replaces "new SomePage()" scattered over all pages.</summary>
internal static class CompositionRoot
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddWaldmannInfrastructure();
        services.AddWaldmannApplication(new DemoAccountInfo(InfrastructureServiceCollectionExtensions.DemoCredentials));

        services.AddSingleton<INavigationService, ShellNavigationService>();
        services.AddSingleton<IDialogService, ShellDialogService>();

        services.AddSingleton<AppShell>();
        services.AddTransient<SignInPage>();
        services.AddTransient<RegistrationPage>();
        services.AddTransient<OffersPage>();
        services.AddTransient<ProfilePage>();
        services.AddTransient<ChatsPage>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
