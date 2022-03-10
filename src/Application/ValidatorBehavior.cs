using FluentValidation;
using MediatR;
using ValidationException = Domain.Exceptions.ValidationException;

namespace Application;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        foreach (var validator in _validators)
        {
            var result = validator.Validate(request);
            if (result.IsValid) continue;
            var topError = result.Errors.First();
            var error = topError.ErrorMessage;
            throw new ValidationException($"validator.{error}");
        }

        return next();
    }
}