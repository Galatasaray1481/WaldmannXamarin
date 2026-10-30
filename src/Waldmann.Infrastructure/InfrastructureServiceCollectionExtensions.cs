using Microsoft.Extensions.DependencyInjection;
using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Infrastructure.InMemory;

namespace Waldmann.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers the in-memory adapters. A server-backed implementation only has to replace these
    /// registrations — nothing above Infrastructure changes.
    /// </summary>
    public static IServiceCollection AddWaldmannInfrastructure(this IServiceCollection services)
    {
        Guard.NotNull(services);
        services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
        services.AddSingleton<IOfferRepository, InMemoryOfferRepository>();
        services.AddSingleton<IChatRepository, InMemoryChatRepository>();
        return services;
    }

    /// <summary>Login of the seeded demo account, shown on the sign-in page while there is no server.</summary>
    public static Credentials DemoCredentials => InMemoryAccountRepository.DemoCredentials;
}
