namespace Architecture.Tests.Application;

public sealed class ApplicationCommandHandlersArchTests
{
    private PredicateList TypesUnderTest => TestRules.ApplicationCommandHandlers;
    private string TypeLabel => "command handlers";
    private string NamespaceName => "Commands";
    private string NameSuffix => "Handler";

    [Fact]
    public void Application_Command_Handlers_Classes_Should_Be_Public()
    {
        var result = TypesUnderTest.Should().BePublic().GetResult();

        result.ShouldBeSuccessful("Application command handlers should be public");
    }

    [Fact]
    public void Application_Command_Handlers_Classes_Should_Be_Sealed()
    {
        var result = TypesUnderTest.Should().BeSealed().GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be sealed");
    }

    [Fact]
    public void Application_Command_Handlers_Classes_Should_Be_In_Correct_Namespace()
    {
        var result = TypesUnderTest
            .Should()
            .BeInOwnNamespace(TestRules.ApplicationAssembly, NamespaceName, NameSuffix)
            .GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be in correct namespace");
    }

    [Fact]
    public void Application_Command_Handlers_Classes_Should_Have_Correct_Name_Suffix()
    {
        var result = TypesUnderTest.Should().HaveNameEndingWith(NameSuffix).GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should have names ending with '{NameSuffix}'");
    }
}
