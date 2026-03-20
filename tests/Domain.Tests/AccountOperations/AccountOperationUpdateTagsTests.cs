using TestFixtures;
using TestFixtures.Domain;

namespace Domain.Tests.AccountOperations;

public class AccountOperationUpdateTagsTests
{
    [Fact]
    public void AccountOperation_UpdateTags_WithTags_ShouldSetTagsAndUpdateTimestamp()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var userId = Guid.NewGuid();
        var tag1 = TagFixture.CreateValidTag(userId, name: "Food", color: "#FF0000");
        var tag2 = TagFixture.CreateValidTag(userId, name: "Bills", color: "#00FF00");
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = operation.UpdateTags([tag1, tag2], updatedAt);

        FixtureBase.AssertSuccess(result);
        operation.Tags.Count.ShouldBe(2);
        operation.Tags.ShouldContain(t => t.Id == tag1.Id);
        operation.Tags.ShouldContain(t => t.Id == tag2.Id);
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void AccountOperation_UpdateTags_WithEmptyCollection_ShouldClearTags()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var userId = Guid.NewGuid();
        var tag = TagFixture.CreateValidTag(userId);
        operation.UpdateTags([tag], FixtureBase.GetTestDate(-1));
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = operation.UpdateTags([], updatedAt);

        FixtureBase.AssertSuccess(result);
        operation.Tags.Count.ShouldBe(0);
        operation.UpdatedAt.ShouldBe(updatedAt);
    }

    [Fact]
    public void AccountOperation_UpdateTags_ShouldReplaceExistingTags()
    {
        var operation = AccountOperationFixture.CreateValidAccountOperation();
        var userId = Guid.NewGuid();
        var firstTag = TagFixture.CreateValidTag(userId, name: "First");
        var secondTag = TagFixture.CreateValidTag(userId, name: "Second");
        operation.UpdateTags([firstTag], FixtureBase.GetTestDate(-1));
        var updatedAt = FixtureBase.GetTestDate(1);

        var result = operation.UpdateTags([secondTag], updatedAt);

        FixtureBase.AssertSuccess(result);
        operation.Tags.Count.ShouldBe(1);
        operation.Tags[0].Id.ShouldBe(secondTag.Id);
        operation.Tags[0].Name.ShouldBe("Second");
    }
}
