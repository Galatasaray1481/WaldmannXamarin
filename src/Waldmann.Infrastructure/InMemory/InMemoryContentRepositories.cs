using Waldmann.Core.Accounts;
using Waldmann.Core.Chats;
using Waldmann.Core.Marketplace;

namespace Waldmann.Infrastructure.InMemory;

/// <summary>The six offers of the legacy "Wildbretkauf" page (<c>Views/MenuCard.xaml</c>), unchanged.</summary>
internal sealed class InMemoryOfferRepository : IOfferRepository
{
    private static readonly IReadOnlyList<GameMeatOffer> Offers =
    [
        new(new OfferId("wildschwein-nackensteak"), "Wildschwein Nackensteaks", GameAnimal.WildBoar, 34.90m, 4, "offer_wildschwein_nackensteak"),
        new(new OfferId("hirsch-rueckenfilet"), "Hirschsteak vom Rückenfilet", GameAnimal.RedDeer, 69.90m, 4, "offer_hirsch_rueckenfilet"),
        new(new OfferId("wildschwein-ruecken"), "Wildschweinrücken ohne Knochen", GameAnimal.WildBoar, 69.90m, 4, "offer_wildschwein_ruecken"),
        new(new OfferId("hirsch-keule"), "Hirschsteak aus Keule", GameAnimal.RedDeer, 69.90m, 4, "offer_hirsch_keule"),
        new(new OfferId("reh-keule"), "Rehsteak aus Keule", GameAnimal.RoeDeer, 69.90m, 4, "offer_reh_keule"),
        new(new OfferId("wildschwein-keule"), "Wildschweinsteak aus Keule", GameAnimal.WildBoar, 69.90m, 4, "offer_wildschwein_keule"),
    ];

    public Task<IReadOnlyList<GameMeatOffer>> GetOffersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Offers);
    }
}

/// <summary>The two sample messages of the legacy <c>PrivateChat.xaml.cs</c>, for every user.</summary>
internal sealed class InMemoryChatRepository : IChatRepository
{
    private static readonly IReadOnlyList<ChatMessage> Messages =
    [
        new("Jana", "Jägerin", "Hallo"),
        new("Flora", "Wildbretkäuferin", "Guten Tag"),
    ];

    public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(UserId userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Messages);
    }
}
