using System.Diagnostics.CodeAnalysis;

namespace Waldmann.Core.Common;

/// <summary>Outcome of an operation that can fail for expected reasons. Exceptions are for bugs.</summary>
public class Result
{
    private static readonly Result SuccessInstance = new(error: null);

    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => Error is not null;

    public static Result Success() => SuccessInstance;

    public static Result Failure(Error error) => new(Guard.NotNull(error));

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>A <see cref="Result"/> that carries a value on success.</summary>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Factory methods are the intended way to create results.")]
public sealed class Result<T> : Result
{
    private readonly T? value;

    private Result(T value)
        : base(error: null) => this.value = value;

    private Result(Error error)
        : base(error)
    {
    }

    /// <exception cref="InvalidOperationException">The result is a failure.</exception>
    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException($"A failed result has no value ({Error.Kind}).");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(Guard.NotNull(error));

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
