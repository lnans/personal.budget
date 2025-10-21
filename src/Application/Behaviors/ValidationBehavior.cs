using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var failures = await ValidateRequest(request, cancellationToken);

        if (failures.Count != 0)
        {
            var error = CreateValidationError(failures);
            return (dynamic)error;
        }

        return await next(cancellationToken);
    }

    private async Task<List<ValidationFailure>> ValidateRequest(TRequest request, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        return validationResults.Where(r => r.Errors.Count != 0).SelectMany(r => r.Errors).ToList();
    }

    private static Error CreateValidationError(List<ValidationFailure> failures)
    {
        var metadata = BuildErrorMetadata(failures);

        return Error.Validation(
            description: "Validation errors occurred.",
            code: "ValidationError",
            metadata: metadata
        );
    }

    private static Dictionary<string, object> BuildErrorMetadata(List<ValidationFailure> failures) =>
        failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                grouping => grouping.Key,
                object (grouping) =>
                    grouping
                        .Select(failure => new { Description = failure.ErrorMessage, Code = failure.ErrorCode })
                        .ToList()
            );
}
