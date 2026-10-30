using System.Text.RegularExpressions;

namespace Waldmann.Core.Validation;

/// <summary>A single violated input rule. Mapped to display text by the presentation layer.</summary>
public enum ValidationIssue
{
    FirstNameRequired,
    LastNameRequired,
    EmailInvalid,
    PasswordRequired,
    PasswordTooShort,
    PasswordsDoNotMatch,
    RoleRequired,
}

/// <summary>
/// Input rules for accounts. The legacy screens had no validation at all (the login page even showed
/// the password in plain text).
/// </summary>
public static class AccountRules
{
    public const int MinimumPasswordLength = 8;

    // netstandard2.0 has no [GeneratedRegex]; a compiled static instance is the equivalent.
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)*\.[A-Za-z]{2,}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailPattern.IsMatch(email!.Trim());

    public static IReadOnlyList<ValidationIssue> ValidateCredentials(string? email, string? password)
    {
        var issues = new List<ValidationIssue>();
        AddIf(issues, !IsValidEmail(email), ValidationIssue.EmailInvalid);
        AddIf(issues, string.IsNullOrEmpty(password), ValidationIssue.PasswordRequired);
        return issues;
    }

    public static IReadOnlyList<ValidationIssue> ValidateRegistration(
        string? firstName,
        string? lastName,
        string? email,
        string? password,
        string? passwordConfirmation,
        bool hasRole)
    {
        var issues = new List<ValidationIssue>();
        AddIf(issues, string.IsNullOrWhiteSpace(firstName), ValidationIssue.FirstNameRequired);
        AddIf(issues, string.IsNullOrWhiteSpace(lastName), ValidationIssue.LastNameRequired);
        AddIf(issues, !IsValidEmail(email), ValidationIssue.EmailInvalid);
        AddIf(issues, (password?.Length ?? 0) < MinimumPasswordLength, ValidationIssue.PasswordTooShort);
        AddIf(issues, !string.Equals(password, passwordConfirmation, StringComparison.Ordinal), ValidationIssue.PasswordsDoNotMatch);
        AddIf(issues, !hasRole, ValidationIssue.RoleRequired);
        return issues;
    }

    private static void AddIf(List<ValidationIssue> issues, bool condition, ValidationIssue issue)
    {
        if (condition)
        {
            issues.Add(issue);
        }
    }
}
