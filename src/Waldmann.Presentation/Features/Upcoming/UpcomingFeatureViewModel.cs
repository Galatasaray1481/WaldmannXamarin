using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation.Features.Upcoming;

/// <summary>Menu entries of the prototype that have no function yet.</summary>
public enum UpcomingFeature
{
    HuntingPermits,
    EventsAndShootingRanges,
    FindHunters,
    HuntingLaw,
    Cart,
}

/// <summary>
/// One honest placeholder for every menu entry without function. The legacy app opened copies of other
/// pages instead — "Events" and "Jäger finden" showed an empty order list, "Jagdrecht" the BurgerTasters
/// "About us" page, "Begehungsscheine" a money pay-in form.
/// </summary>
public sealed class UpcomingFeatureViewModel(UpcomingFeature feature)
{
    public UpcomingFeature Feature { get; } = feature;

    public string Title => Feature switch
    {
        UpcomingFeature.HuntingPermits => Strings.Shell_Permits,
        UpcomingFeature.EventsAndShootingRanges => Strings.Shell_Events,
        UpcomingFeature.FindHunters => Strings.Shell_Hunters,
        UpcomingFeature.HuntingLaw => Strings.Shell_HuntingLaw,
        UpcomingFeature.Cart => Strings.Shell_Cart,
        _ => throw new ArgumentOutOfRangeException(nameof(Feature), Feature, null),
    };

    public string Description => Feature switch
    {
        UpcomingFeature.HuntingPermits => Strings.Upcoming_Permits_Description,
        UpcomingFeature.EventsAndShootingRanges => Strings.Upcoming_Events_Description,
        UpcomingFeature.FindHunters => Strings.Upcoming_Hunters_Description,
        UpcomingFeature.HuntingLaw => Strings.Upcoming_HuntingLaw_Description,
        UpcomingFeature.Cart => Strings.Upcoming_Cart_Description,
        _ => throw new ArgumentOutOfRangeException(nameof(Feature), Feature, null),
    };

    public string Status { get; } = Strings.Common_InPreparation;
}
