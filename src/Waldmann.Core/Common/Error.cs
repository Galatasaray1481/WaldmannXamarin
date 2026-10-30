using Waldmann.Core.Validation;

namespace Waldmann.Core.Common;

/// <summary>Why an operation failed. Display texts are produced by the presentation layer only.</summary>
public enum ErrorKind
{
    /// <summary>User input violated a rule; see <see cref="Error.Issues"/>.</summary>
    Validation,

    /// <summary>E-mail address or password is wrong.</summary>
    InvalidCredentials,

    /// <summary>The e-mail address is already registered.</summary>
    AccountAlreadyExists,

    /// <summary>The operation requires a signed-in user.</summary>
    NotSignedIn,
}

/// <summary>Describes a failed operation.</summary>
public sealed record Error
{
    private Error(ErrorKind kind, IReadOnlyList<ValidationIssue> issues)
    {
        Kind = kind;
        Issues = issues;
    }

    public ErrorKind Kind { get; }

    public IReadOnlyList<ValidationIssue> Issues { get; }

    public static Error Of(ErrorKind kind) =>
        kind == ErrorKind.Validation
            ? throw new ArgumentException("Use Error.Validation(...) to create validation errors.", nameof(kind))
            : new Error(kind, []);

    public static Error Validation(IReadOnlyList<ValidationIssue> issues)
    {
        Guard.NotNull(issues);
        return issues.Count == 0
            ? throw new ArgumentException("A validation error needs at least one issue.", nameof(issues))
            : new Error(ErrorKind.Validation, issues);
    }
}
