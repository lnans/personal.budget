using Domain.Tags;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Tags;

public class TagDeleteTests
{
    [Fact]
    public void Tag_Delete_WithValidTimestamp_ShouldMarkTagAsDeleted()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var deletedAt = FixtureBase.GetTestDate(1);

        var result = tag.Delete(deletedAt);

        FixtureBase.AssertSuccess(result);
        tag.DeletedAt.ShouldNotBeNull();
        tag.DeletedAt.ShouldBe(deletedAt);
        tag.UpdatedAt.ShouldBe(deletedAt);
    }

    [Fact]
    public void Tag_Delete_WhenAlreadyDeleted_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var firstDeleteAt = FixtureBase.GetTestDate(1);
        tag.Delete(firstDeleteAt);

        var secondDeleteAt = FixtureBase.GetTestDate(2);
        var result = tag.Delete(secondDeleteAt);

        FixtureBase.AssertError(result, TagErrors.TagAlreadyDeleted);
        tag.DeletedAt.ShouldBe(firstDeleteAt);
    }
}
