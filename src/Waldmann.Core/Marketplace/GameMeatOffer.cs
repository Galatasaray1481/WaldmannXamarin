namespace Waldmann.Core.Marketplace;

public enum GameAnimal
{
    WildBoar,
    RedDeer,
    RoeDeer,
}

/// <summary>Identifies an offer.</summary>
public readonly record struct OfferId(string Value)
{
    public override string ToString() => Value;
}

/// <summary>
/// A game-meat offer of the "Wildbretkauf" page. Replaces six hand-placed tiles in the legacy
/// <c>MenuCard.xaml</c>, whose names and prices were XAML literals. <see cref="ImageKey"/> is a neutral key
/// the UI resolves to an image.
/// </summary>
public sealed record GameMeatOffer(
    OfferId Id,
    string Title,
    GameAnimal Animal,
    decimal PricePerKg,
    int Rating,
    string ImageKey)
{
    public const int MaximumRating = 5;
}

/// <summary>Port for the offers.</summary>
public interface IOfferRepository
{
    Task<IReadOnlyList<GameMeatOffer>> GetOffersAsync(CancellationToken cancellationToken);
}

/// <summary>Offers sorted by animal, then price — the legacy page had a fixed, unsorted grid.</summary>
public sealed class GetOffersUseCase(IOfferRepository offers)
{
    public async Task<IReadOnlyList<GameMeatOffer>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var all = await offers.GetOffersAsync(cancellationToken).ConfigureAwait(false);
        return [.. all.OrderBy(offer => offer.Animal).ThenBy(offer => offer.PricePerKg).ThenBy(offer => offer.Title, StringComparer.Ordinal)];
    }
}
