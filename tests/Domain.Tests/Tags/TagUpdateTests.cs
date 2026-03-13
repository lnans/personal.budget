using Domain.Tags;
using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.Tags;

public class TagUpdateTests
{
    [Fact]
    public void Tag_Update_WithValidParameters_ShouldUpdateTag()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        const string newName = "Updated Tag";
        const string newColor = "#00FF00";

        var result = tag.Update(newName, newColor, updatedAt);

        FixtureBase.AssertSuccess(result);
        tag.Name.ShouldBe(newName);
        tag.Color.ShouldBe(newColor);
        tag.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void Tag_Update_WithEmptyName_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = tag.Update("", "#00FF00", updatedAt);

        FixtureBase.AssertError(result, TagErrors.TagNameRequired);
    }

    [Fact]
    public void Tag_Update_WithTooLongName_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);
        var newName = TagFixture.GenerateLongTagName();

        var result = tag.Update(newName, "#00FF00", updatedAt);

        FixtureBase.AssertError(result, TagErrors.TagNameTooLong);
    }

    [Fact]
    public void Tag_Update_WithEmptyColor_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = tag.Update("Updated Tag", "", updatedAt);

        FixtureBase.AssertError(result, TagErrors.TagColorRequired);
    }

    [Fact]
    public void Tag_Update_WithInvalidColor_ShouldReturnError()
    {
        var user = UserFixture.CreateValidUser();
        var tag = TagFixture.CreateValidTag(user.Id);
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = tag.Update("Updated Tag", "invalid", updatedAt);

        FixtureBase.AssertError(result, TagErrors.TagColorInvalid);
    }
}
