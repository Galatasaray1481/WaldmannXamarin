using System.Collections.ObjectModel;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Presentation.Navigation;

namespace Waldmann.Presentation.Features.Offers;

/// <summary>One tile of the "Wildbretkauf" page.</summary>
public sealed record OfferTile(string Title, string AnimalText, string PriceText, string RatingStars, string RatingDescription, string ImageKey)
{
    public static OfferTile From(GameMeatOffer offer)
    {
        Guard.NotNull(offer);
        return new OfferTile(
            offer.Title,
            DisplayFormat.Animal(offer.Animal),
            DisplayFormat.PricePerKg(offer.PricePerKg),
            DisplayFormat.RatingStars(offer.Rating),
            DisplayFormat.RatingDescription(offer.Rating),
            offer.ImageKey);
    }
}

/// <summary>
/// Replaces <c>Views/MenuCard.xaml(.cs)</c> and the five identical bottom tabs of <c>BottomNavBar.xaml</c>
/// that all showed this same page. The tap handlers of the legacy tiles had their bodies commented out;
/// the tiles are therefore not tappable (an offer detail page is not part of the prototype).
/// </summary>
public sealed partial class OffersViewModel(GetOffersUseCase getOffers, IDialogService dialogs) : ViewModelBase(dialogs), IAppearingAware
{
    public ObservableCollection<OfferTile> Offers { get; } = [];

    public Task OnAppearingAsync() => Offers.Count > 0 ? Task.CompletedTask : RunBusyAsync(async () =>
    {
        foreach (var offer in await getOffers.ExecuteAsync())
        {
            Offers.Add(OfferTile.From(offer));
        }
    });
}
