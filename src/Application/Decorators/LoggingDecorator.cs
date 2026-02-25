using Application.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace Application.Decorators;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _innerHandler;
        private readonly ILogger<ICommandHandler<TCommand>> _logger;

        public CommandHandler(ICommandHandler<TCommand> innerHandler, ILogger<ICommandHandler<TCommand>> logger)
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;

            _logger.LogInformation("Processing command {Command}", commandName);

            var result = await _innerHandler.Handle(command, cancellationToken);

            if (result.IsError)
            {
                _logger.LogError(
                    "Completed command {Command} with errors: {Errors}",
                    commandName,
                    result.Errors.Select(e => e.Code)
                );
            }
            else
            {
                _logger.LogInformation("Completed command {Command}", commandName);
            }

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _innerHandler;
        private readonly ILogger<ICommandHandler<TCommand, TResponse>> _logger;

        public CommandHandler(
            ICommandHandler<TCommand, TResponse> innerHandler,
            ILogger<ICommandHandler<TCommand, TResponse>> logger
        )
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;

            _logger.LogInformation("Processing command {Command}", commandName);

            var result = await _innerHandler.Handle(command, cancellationToken);

            if (result.IsError)
            {
                _logger.LogError(
                    "Completed command {Command} with errors: {Errors}",
                    commandName,
                    result.Errors.Select(e => e.Code)
                );
            }
            else
            {
                _logger.LogInformation("Completed command {Command}", commandName);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly IQueryHandler<TQuery, TResponse> _innerHandler;
        private readonly ILogger<IQueryHandler<TQuery, TResponse>> _logger;

        public QueryHandler(
            IQueryHandler<TQuery, TResponse> innerHandler,
            ILogger<IQueryHandler<TQuery, TResponse>> logger
        )
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var queryName = typeof(TQuery).Name;

            _logger.LogInformation("Processing query {Query}", queryName);

            var result = await _innerHandler.Handle(query, cancellationToken);

            if (result.IsError)
            {
                _logger.LogError(
                    "Completed query {Query} with errors: {Errors}",
                    queryName,
                    result.Errors.Select(e => e.Code)
                );
            }
            else
            {
                _logger.LogInformation("Completed query {Query}", queryName);
            }

            return result;
        }
    }
}
