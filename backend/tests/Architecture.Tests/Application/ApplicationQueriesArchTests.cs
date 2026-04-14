namespace Architecture.Tests.Application;

public sealed class ApplicationQueriesArchTests
{
    private PredicateList TypesUnderTest => TestRules.ApplicationQueries;
    private string TypeLabel => "queries";
    private string NamespaceName => "Queries";
    private string NameSuffix => "Query";

    [Fact]
    public void Application_Queries_Classes_Should_Be_Public()
    {
        var result = TypesUnderTest.Should().BePublic().GetResult();

        result.ShouldBeSuccessful("Application queries should be public");
    }

    [Fact]
    public void Application_Queries_Classes_Should_Be_Record()
    {
        var result = TypesUnderTest.Should().BeRecord(TestRules.ApplicationAssembly).GetResult();

        result.ShouldBeSuccessful("Application queries should be immutable records");
    }

    [Fact]
    public void Application_Queries_Classes_Should_Be_Sealed()
    {
        var result = TypesUnderTest.Should().BeSealed().GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be sealed");
    }

    [Fact]
    public void Application_Queries_Classes_Should_Be_In_Correct_Namespace()
    {
        var result = TypesUnderTest
            .Should()
            .BeInOwnNamespace(TestRules.ApplicationAssembly, NamespaceName, NameSuffix)
            .GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should be in correct namespace");
    }

    [Fact]
    public void Application_Queries_Classes_Should_Have_Correct_Name_Suffix()
    {
        var result = TypesUnderTest.Should().HaveNameEndingWith(NameSuffix).GetResult();

        result.ShouldBeSuccessful($"Application {TypeLabel} should have names ending with '{NameSuffix}'");
    }
}
