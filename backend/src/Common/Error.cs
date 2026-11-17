using System.Diagnostics.CodeAnalysis;

namespace Personal.Budget.App.Common;

internal enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
}

internal readonly record struct Error(ErrorType Type, string Code, string Description, string? Field = null)
{
    public static Error Failure(string code, string description) => new(ErrorType.Failure, code, description);

    public static Error Validation(string code, string description) => new(ErrorType.Validation, code, description);

    public static Error NotFound(string code, string description) => new(ErrorType.NotFound, code, description);

    public static Error Conflict(string code, string description) => new(ErrorType.Conflict, code, description);

    public static Dictionary<string, string[]> ToProblemErrors(IReadOnlyList<Error> errors) =>
        errors
            .GroupBy(e => e.Field ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
}

internal readonly struct ErrorOr<T>
{
    private readonly T? _value;

    private ErrorOr(T value)
    {
        _value = value;
        Errors = null;
        IsSuccess = true;
    }

    private ErrorOr(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
        {
            throw new ArgumentException("An error result needs at least one error.", nameof(errors));
        }
        _value = default;
        Errors = errors;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    [AllowNull]
    public IReadOnlyList<Error> Errors => field ?? [];

    public static implicit operator ErrorOr<T>(T value) => new(value);

    public static implicit operator ErrorOr<T>(Error error) => new([error]);

    public static implicit operator ErrorOr<T>(List<Error> errors) => new(errors);

    public TOut Match<TOut>(Func<T, TOut> ok, Func<IReadOnlyList<Error>, TOut> fail) =>
        IsSuccess ? ok(_value!) : fail(Errors);

    public ErrorOr<TOut> Map<TOut>(Func<T, TOut> f) => IsSuccess ? f(_value!) : new ErrorOr<TOut>(Errors);

    public ErrorOr<TOut> Then<TOut>(Func<T, ErrorOr<TOut>> f) => IsSuccess ? f(_value!) : new ErrorOr<TOut>(Errors);

    public Task<ErrorOr<TOut>> ThenAsync<TOut>(Func<T, Task<ErrorOr<TOut>>> f) =>
        IsSuccess ? f(_value!) : Task.FromResult(new ErrorOr<TOut>(Errors));


}
