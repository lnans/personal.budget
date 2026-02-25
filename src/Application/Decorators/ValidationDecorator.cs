using Application.Extensions;
using Application.Interfaces;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace Application.Decorators;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly IValidator<TCommand>[] _validators;
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;

        public CommandHandler(
            IEnumerable<IValidator<TCommand>> validators,
            ICommandHandler<TCommand, TResponse> innerHandler,
            ILogger<CommandHandler<TCommand, TResponse>> logger
        )
        {
            _validators = validators as IValidator<TCommand>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
            _logger = logger;
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
            var commandName = typeof(TCommand).Name;

            _logger.LogInformation("Validating command {Command}", commandName);

            var failures = await ValidateCommand(command, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(command, cancellationToken);
            }

            _logger.LogError(
                "Validation failed for {Command} with errors: {Errors}",
                commandName,
                failures.Select(failure => failure.ErrorCode)
            );

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
        private readonly ILogger<CommandHandler<TCommand>> _logger;

        public CommandHandler(
            IEnumerable<IValidator<TCommand>> validators,
            ICommandHandler<TCommand> innerHandler,
            ILogger<CommandHandler<TCommand>> logger
        )
        {
            _validators = validators as IValidator<TCommand>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
            _logger = logger;
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
            var commandName = typeof(TCommand).Name;

            _logger.LogInformation("Validating command {Command}", commandName);

            var failures = await ValidateCommand(command, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(command, cancellationToken);
            }

            _logger.LogError(
                "Validation failed for {Command} with errors: {Errors}",
                commandName,
                failures.Select(failure => failure.ErrorCode)
            );

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
        private readonly ILogger<IQueryHandler<TQuery, TResponse>> _logger;

        public QueryHandler(
            IEnumerable<IValidator<TQuery>> validators,
            IQueryHandler<TQuery, TResponse> innerHandler,
            ILogger<IQueryHandler<TQuery, TResponse>> logger
        )
        {
            _validators = validators as IValidator<TQuery>[] ?? validators.ToArray();
            _innerHandler = innerHandler;
            _logger = logger;
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
            var queryName = typeof(TQuery).Name;

            _logger.LogInformation("Validating query {Query}", queryName);

            var failures = await ValidateQuery(query, cancellationToken);
            if (failures.Count == 0)
            {
                return await _innerHandler.Handle(query, cancellationToken);
            }

            _logger.LogError(
                "Validation failed for {Query} with errors: {Errors}",
                queryName,
                failures.Select(failure => failure.ErrorCode)
            );

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
