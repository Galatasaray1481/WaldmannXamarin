using System.Globalization;
using Waldmann.Core.Accounts;
using Waldmann.Core.Common;
using Waldmann.Core.Marketplace;
using Waldmann.Core.Validation;
using Waldmann.Presentation.Resources;

namespace Waldmann.Presentation;

/// <summary>Turns an <see cref="Error"/> into the text the user sees.</summary>
public static class ErrorMessages
{
    public static string For(Error error)
    {
        Guard.NotNull(error);
        return error.Kind switch
        {
            ErrorKind.Validation => string.Join(Environment.NewLine, error.Issues.Select(For)),
            ErrorKind.InvalidCredentials => Strings.Error_InvalidCredentials,
            ErrorKind.AccountAlreadyExists => Strings.Error_AccountAlreadyExists,
            ErrorKind.NotSignedIn => Strings.Error_NotSignedIn,
            _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, null),
        };
    }

    public static string For(ValidationIssue issue) => issue switch
    {
        ValidationIssue.FirstNameRequired => Strings.Validation_FirstNameRequired,
        ValidationIssue.LastNameRequired => Strings.Validation_LastNameRequired,
        ValidationIssue.EmailInvalid => Strings.Validation_EmailInvalid,
        ValidationIssue.PasswordRequired => Strings.Validation_PasswordRequired,
        ValidationIssue.PasswordTooShort => DisplayFormat.Format(Strings.Validation_PasswordTooShort, AccountRules.MinimumPasswordLength),
        ValidationIssue.PasswordsDoNotMatch => Strings.Validation_PasswordsDoNotMatch,
        ValidationIssue.RoleRequired => Strings.Validation_RoleRequired,
        _ => throw new ArgumentOutOfRangeException(nameof(issue), issue, null),
    };
}

/// <summary>How prices, ratings and enum values are shown. Always German formatting.</summary>
public static class DisplayFormat
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>"34,90 € / kg". Built explicitly: the separator of the "C" format differs between platforms.</summary>
    public static string PricePerKg(decimal price) =>
        Format(Strings.Offers_PricePerKg, price.ToString("#,##0.00", Culture) + "\u00A0€");

    /// <summary>"★★★★☆" — replaces the static image fourStars.png that every legacy tile showed.</summary>
    public static string RatingStars(int rating)
    {
        var filled = Math.Max(0, Math.Min(GameMeatOffer.MaximumRating, rating));
        return new string('★', filled) + new string('☆', GameMeatOffer.MaximumRating - filled);
    }

    public static string RatingDescription(int rating) =>
        Format(Strings.Offers_RatingDescription, rating, GameMeatOffer.MaximumRating);

    public static string Role(UserRole role) => role switch
    {
        UserRole.Hunter => Strings.Role_Hunter,
        UserRole.GameMeatBuyer => Strings.Role_GameMeatBuyer,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
    };

    public static string Animal(GameAnimal animal) => animal switch
    {
        GameAnimal.WildBoar => Strings.Animal_WildBoar,
        GameAnimal.RedDeer => Strings.Animal_RedDeer,
        GameAnimal.RoeDeer => Strings.Animal_RoeDeer,
        _ => throw new ArgumentOutOfRangeException(nameof(animal), animal, null),
    };

    public static string Format(string format, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, format, arguments);
}
