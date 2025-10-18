namespace Api.Tests;

[CollectionDefinition(CollectionName)]
public class ApiTestCollection : ICollectionFixture<ApiTestFixture>
{
    public const string CollectionName = "ApiTestCollection";
}
