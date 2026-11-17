using System.Text.RegularExpressions;

namespace Personal.Budget.App.Common;

internal sealed class Validation
{
    private readonly List<Error> _errors = [];

    private Validation() { }

    public static Validation Begin() => new();

    public Validation Check<T>(T value, string field, Func<Rule<T>, Rule<T>> rules)
    {
        if (rules(new Rule<T>(value)).Error is { } error)
        {
            _errors.Add(error with { Field = field });
        }
        return this;
    }

    public ErrorOr<T> Done<T>(Func<T> factory) => _errors.Count == 0 ? factory() : _errors;
}

internal readonly record struct Rule<T>(T Value, Error? Error = null)
{
    public Rule<T> Must(Func<T, bool> predicate, Error error) =>
        Error is null && !predicate(Value) ? this with { Error = error } : this;
}

internal static class StringRules
{
    extension(Rule<string?> r)
    {
        public Rule<string?> NotEmpty(Error error) => r.Must(v => !string.IsNullOrWhiteSpace(v), error);

        public Rule<string?> MaxLength(int max, Error error) => r.Must(v => v is null || v.Length <= max, error);

        public Rule<string?> Matches(Regex regex, Error error) => r.Must(v => v is not null && regex.IsMatch(v), error);
    }
}
