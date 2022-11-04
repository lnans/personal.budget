using Xunit;

namespace Personal.Budget.Api.IntegrationTests;

[CollectionDefinition("Shared")]
public class SharedTestCollection : ICollectionFixture<ApiFactory>
{
}