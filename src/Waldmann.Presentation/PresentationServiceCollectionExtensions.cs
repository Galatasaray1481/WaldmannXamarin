using Microsoft.Extensions.DependencyInjection;
using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Presentation.Features;
using Waldmann.Presentation.Features.Chats;
using Waldmann.Presentation.Features.Offers;
using Waldmann.Presentation.Features.Profile;
using Waldmann.Presentation.Features.Registration;
using Waldmann.Presentation.Features.SignIn;

namespace Waldmann.Presentation;

public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the session, the use cases and the ViewModels — everything above the adapters and
    /// below Xamarin.Forms. The app and the integration tests build their containers from this method.
    /// </summary>
    public static IServiceCollection AddWaldmannApplication(this IServiceCollection services, DemoAccountInfo demoAccount)
    {
        Guard.NotNull(services);
        services.AddSingleton(Guard.NotNull(demoAccount));
        services.AddSingleton<IUserSession, UserSession>();

        services.AddTransient<SignInUseCase>();
        services.AddTransient<RegisterUseCase>();
        services.AddTransient<GetOffersUseCase>();
        services.AddTransient<GetChatMessagesUseCase>();

        services.AddSingleton<AppShellViewModel>();
        services.AddTransient<SignInViewModel>();
        services.AddTransient<RegistrationViewModel>();
        services.AddTransient<OffersViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ChatsViewModel>();
        return services;
    }
}
