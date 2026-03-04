using System.Reflection;
using Application;
using Application.Interfaces;
using FluentValidation;

namespace Architecture.Tests;

public static class TestRules
{
    public static readonly Assembly ApplicationAssembly = Assembly.GetAssembly(typeof(DependencyInjection))!;

    public static readonly PredicateList ApplicationDecorators = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(IQueryHandler<,>))
        .And()
        .AreNested() // only for decorators
        .Or()
        .ImplementInterface(typeof(ICommandHandler<>))
        .And()
        .AreNested() // only for decorators
        .Or()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .And()
        .AreNested(); // only for decorators

    public static readonly PredicateList ApplicationValidators = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .Inherit(typeof(AbstractValidator<>));

    public static readonly PredicateList ApplicationQueries = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(IQuery<>));

    public static readonly PredicateList ApplicationQueryHandlers = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(IQueryHandler<,>))
        .And()
        .AreNotNested(); // exclude decorators

    public static readonly PredicateList ApplicationCommands = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(ICommand<>))
        .Or()
        .ImplementInterface(typeof(ICommand));

    public static readonly PredicateList ApplicationCommandHandlers = Types
        .InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(ICommandHandler<>))
        .And()
        .AreNotNested() // exclude decorators
        .Or()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .And()
        .AreNotNested(); // exclude decorators
}
