using Application.Extensions;
using Application.Interfaces;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Decorators;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly IValidator<TCommand>[] _validators;
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;

        public CommandHandler(
            IEnumerable<IValidator<TCommand>> validators,
            ICommandHandler<TCommand, TResponse> innerHandler
        )
        {
            _validators = validators as IValidator<TCommand>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
        }

        public Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            if (_validators.Length == 0)
            {
                return _innerHandler.Handle(command, cancellationToken);
            }

            return HandleWithValidation(command, cancellationToken);
        }

        private async Task<ErrorOr<TResponse>> HandleWithValidation(
            TCommand command,
            CancellationToken cancellationToken
        )
        {
            var failures = await ValidateCommand(command, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(command, cancellationToken);
            }

            var error = failures.CreateValidationError();
            return error;
        }

        private async Task<List<ValidationFailure>> ValidateCommand(
            TCommand command,
            CancellationToken cancellationToken
        )
        {
            var context = new ValidationContext<TCommand>(command);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            return validationResults.Where(r => r.Errors.Count != 0).SelectMany(r => r.Errors).ToList();
        }
    }

    internal sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IValidator<TCommand>[] _validators;
        private readonly ICommandHandler<TCommand> _innerHandler;

        public CommandHandler(IEnumerable<IValidator<TCommand>> validators, ICommandHandler<TCommand> innerHandler)
        {
            _validators = validators as IValidator<TCommand>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
        }

        public Task<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            if (_validators.Length == 0)
            {
                return _innerHandler.Handle(command, cancellationToken);
            }

            return HandleWithValidation(command, cancellationToken);
        }

        private async Task<ErrorOr<Success>> HandleWithValidation(TCommand command, CancellationToken cancellationToken)
        {
            var failures = await ValidateCommand(command, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(command, cancellationToken);
            }

            var error = failures.CreateValidationError();
            return error;
        }

        private async Task<List<ValidationFailure>> ValidateCommand(
            TCommand command,
            CancellationToken cancellationToken
        )
        {
            var context = new ValidationContext<TCommand>(command);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            return validationResults.Where(r => r.Errors.Count != 0).SelectMany(r => r.Errors).ToList();
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly IValidator<TQuery>[] _validators;
        private readonly IQueryHandler<TQuery, TResponse> _innerHandler;

        public QueryHandler(IEnumerable<IValidator<TQuery>> validators, IQueryHandler<TQuery, TResponse> innerHandler)
        {
            _validators = validators as IValidator<TQuery>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
        }

        public Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            if (_validators.Length == 0)
            {
                return _innerHandler.Handle(query, cancellationToken);
            }

            return HandleWithValidation(query, cancellationToken);
        }

        private async Task<ErrorOr<TResponse>> HandleWithValidation(TQuery query, CancellationToken cancellationToken)
        {
            var failures = await ValidateQuery(query, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(query, cancellationToken);
            }

            var error = failures.CreateValidationError();
            return error;
        }

        private async Task<List<ValidationFailure>> ValidateQuery(TQuery query, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TQuery>(query);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            return validationResults.Where(r => r.Errors.Count != 0).SelectMany(r => r.Errors).ToList();
        }
    }
}
