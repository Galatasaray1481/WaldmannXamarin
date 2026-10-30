using System.Runtime.CompilerServices;

namespace Waldmann.Core.Common;

/// <summary>Argument checks for netstandard2.0, which lacks <c>ArgumentNullException.ThrowIfNull</c>.</summary>
public static class Guard
{
    public static T NotNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : class =>
        value ?? throw new ArgumentNullException(name);
}
