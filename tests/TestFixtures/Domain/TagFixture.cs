using Domain.Tags;

namespace TestFixtures.Domain;

public static class TagFixture
{
    public static Tag CreateValidTag(
        Guid userId,
        string name = "Test Tag",
        string color = "#FF5733",
        DateTimeOffset? createdAt = null
    ) => Tag.Create(userId, name, color, createdAt ?? DateTimeOffset.UtcNow).Value;

    public static string GenerateLongTagName() => FixtureBase.GenerateLongString(TagConstants.MaxNameLength + 1);
}
