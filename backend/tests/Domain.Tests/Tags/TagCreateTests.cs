using Domain.Tags;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Tags;

public class TagCreateTests
{
    [Fact]
    public void Tag_Create_WithValidParameters_ShouldCreateTag()
    {
        var user = UserFixture.CreateValidUser();
        const string tagName = "Groceries";
        const string tagColor = "#FF5733";
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, tagName, tagColor, createdAt);

        tag.IsError.ShouldBeFalse();
        tag.Value.UserId.ShouldBe(user.Id);
        tag.Value.Name.ShouldBe(tagName);
        tag.Value.Color.ShouldBe(tagColor);
        tag.Value.CreatedAt.ShouldBe(createdAt);
        tag.Value.UpdatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public void Tag_Create_WithEmptyName_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, "", "#FF5733", createdAt);

        FixtureBase.AssertError(tag, TagErrors.TagNameRequired);
    }

    [Fact]
    public void Tag_Create_WithTooLongName_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tagName = TagFixture.GenerateLongTagName();
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, tagName, "#FF5733", createdAt);

        FixtureBase.AssertError(tag, TagErrors.TagNameTooLong);
    }

    [Fact]
    public void Tag_Create_WithEmptyColor_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, "Groceries", "", createdAt);

        FixtureBase.AssertError(tag, TagErrors.TagColorRequired);
    }

    [Theory]
    [InlineData("FF5733")]
    [InlineData("#GG5733")]
    [InlineData("#FF573")]
    [InlineData("#FF57331")]
    [InlineData("red")]
    public void Tag_Create_WithInvalidColor_ShouldReturnError(string color)
    {
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, "Groceries", color, createdAt);

        FixtureBase.AssertError(tag, TagErrors.TagColorInvalid);
    }

    [Theory]
    [InlineData("#000000")]
    [InlineData("#FFFFFF")]
    [InlineData("#ff5733")]
    [InlineData("#aAbBcC")]
    public void Tag_Create_WithValidColor_ShouldCreateTag(string color)
    {
        var user = UserFixture.CreateValidUser();
        var createdAt = FixtureBase.GetTestDate();

        var tag = Tag.Create(user.Id, "Groceries", color, createdAt);

        tag.IsError.ShouldBeFalse();
        tag.Value.Color.ShouldBe(color);
    }
}
