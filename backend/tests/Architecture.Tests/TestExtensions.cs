using System.Reflection;
using Mono.Cecil;
using Xunit.Sdk;
using TestResult = NetArchTest.Rules.TestResult;

namespace Architecture.Tests;

public static class TestExtensions
{
    public static void ShouldBeSuccessful(this TestResult result, string errorMessage)
    {
        if (result.IsSuccessful)
        {
            return;
        }

        var failingTypes = result.FailingTypeNames.Select(t => $"- {t.Split(".").Last()}");
        var message =
            $"{errorMessage}:{Environment.NewLine}{string.Join(Environment.NewLine, failingTypes)}{Environment.NewLine}";
        throw new XunitException(message);
    }

    public static ConditionList BeRecord(this Conditions conditions, Assembly assembly) =>
        conditions.MeetCustomRule(new ReflectionRule(assembly, type => type.IsImmutableRecord()));

    public static ConditionList BeInOwnNamespace(
        this Conditions conditions,
        Assembly assembly,
        string namespaceName,
        string name
    ) => conditions.MeetCustomRule(new ReflectionRule(assembly, type => type.IsInOwnNamespace(namespaceName, name)));

    public static ConditionList BeInternal(this Conditions conditions, Assembly assembly) =>
        conditions.MeetCustomRule(new ReflectionRule(assembly, type => type.IsInternal()));

    public static ConditionList HaveGenericNameEndingWith(this Conditions conditions, Assembly assembly, string name) =>
        conditions.MeetCustomRule(new ReflectionRule(assembly, type => type.HasNameEndingWith(name)));

    private sealed class ReflectionRule : ICustomRule
    {
        private readonly Assembly _assembly;
        private readonly Func<Type, bool> _predicate;

        public ReflectionRule(Assembly assembly, Func<Type, bool> predicate)
        {
            _assembly = assembly;
            _predicate = predicate;
        }

        public bool MeetsRule(TypeDefinition type)
        {
            var resolvedType = _assembly.GetType(type.FullName.Replace("/", "+"));
            return resolvedType is not null && _predicate(resolvedType);
        }
    }

    private static bool IsImmutableRecord(this Type type)
    {
        var isRecord = type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic) is not null;

        if (!isRecord)
        {
            return false;
        }

        var mutableProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
            {
                var setter = p.SetMethod;
                if (setter is null)
                {
                    return false;
                }

                return !setter
                    .ReturnParameter.GetRequiredCustomModifiers()
                    .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
            })
            .Select(p => p.Name)
            .ToList();

        return mutableProperties.Count == 0;
    }

    private static bool IsInOwnNamespace(this Type type, string namespaceName, string name)
    {
        var requestName = !string.IsNullOrWhiteSpace(name) ? type.Name.Replace(name, "").Split("`")[0] : string.Empty;
        var requestNamespace = type.Namespace;
        return string.IsNullOrWhiteSpace(requestName)
            ? requestNamespace!.EndsWith(namespaceName)
            : requestNamespace!.EndsWith($"{namespaceName}.{requestName}");
    }

    private static bool IsInternal(this Type type) => (type.IsNotPublic || type.IsNested) && !type.IsVisible;

    private static bool HasNameEndingWith(this Type type, string suffix) =>
        type.FullName!.Split("+")[0].EndsWith(suffix);
}
