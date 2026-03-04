namespace Architecture.Tests.Application;

public sealed class ApplicationValidatorsArchTests
{
    private PredicateList TypesUnderTest => TestRules.ApplicationValidators;
    private string TypeLabel => "validators";
    private string NamespaceQueriesName => "Queries";
    private string NamespaceCommandsName => "Commands";
    private string NameSuffix => "Validator";

    [Fact]
    public void Application_Validators_Classes_Should_Be_Internal()
    {
        var result = TypesUnderTest.Should().BeInternal(TestRules.ApplicationAssembly).GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be internal");
    }

    [Fact]
    public void Application_Validators_Classes_Should_Be_Sealed()
    {
        var result = TypesUnderTest.Should().BeSealed().GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be sealed");
    }

    [Fact]
    public void Application_Validators_Classes_Should_Be_In_Correct_Namespace()
    {
        var resultQueries = TypesUnderTest
            .And()
            .ResideInNamespaceContaining(".*Queries.*")
            .Should()
            .BeInOwnNamespace(TestRules.ApplicationAssembly, NamespaceQueriesName, NameSuffix)
            .GetResult();

        var resultCommands = TypesUnderTest
            .And()
            .ResideInNamespaceContaining(".*Commands.*")
            .Should()
            .BeInOwnNamespace(TestRules.ApplicationAssembly, NamespaceCommandsName, NameSuffix)
            .GetResult();

        resultQueries.ShouldBeSuccessful($"Application queries {TypeLabel} should be in correct namespace");
        resultCommands.ShouldBeSuccessful($"Application commands {TypeLabel} should be in correct namespace");
    }

    [Fact]
    public void Application_Validators_Classes_Should_Have_Correct_Name_Suffix()
    {
        var result = TypesUnderTest.Should().HaveNameEndingWith(NameSuffix).GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should have names ending with '{NameSuffix}'");
    }
}
