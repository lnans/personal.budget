namespace Architecture.Tests.Application;

public sealed class ApplicationDecoratorsArchTests
{
    private PredicateList TypesUnderTest => TestRules.ApplicationDecorators;
    private string TypeLabel => "decorators";
    private string NamespaceName => "Decorators";
    private string NameSuffix => "Decorator";
    private string NamespaceNameSuffix => "";

    [Fact]
    public void Application_Decorators_Classes_Should_Be_Internal()
    {
        var result = TypesUnderTest.Should().BeInternal(TestRules.ApplicationAssembly).GetResult();

        result.ShouldBeSuccessful("Application decorators should be internal");
    }

    [Fact]
    public void Application_Decorators_Classes_Should_Be_Sealed()
    {
        var result = TypesUnderTest.Should().BeSealed().GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be sealed");
    }

    [Fact]
    public void Application_Decorators_Classes_Should_Be_In_Correct_Namespace()
    {
        var result = TypesUnderTest
            .Should()
            .BeInOwnNamespace(TestRules.ApplicationAssembly, NamespaceName, NamespaceNameSuffix)
            .GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be in correct namespace");
    }

    [Fact]
    public void Application_Decorators_Classes_Should_Have_Correct_Name_Suffix()
    {
        var result = TypesUnderTest
            .Should()
            .HaveGenericNameEndingWith(TestRules.ApplicationAssembly, NameSuffix)
            .GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should have names ending with '{NameSuffix}'");
    }
}
