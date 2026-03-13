using Domain.Users;
using ErrorOr;

namespace Domain.Tags;

public sealed class Tag : Entity
{
    public Guid UserId { get; }
    public string Name { get; private set; }
    public string Color { get; private set; }

    public User User { get; } = null!;

    private Tag(Guid userId, string name, string color, DateTimeOffset createdAt)
        : base(createdAt)
    {
        UserId = userId;
        Name = name;
        Color = color;
    }

    public static ErrorOr<Tag> Create(Guid userId, string name, string color, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TagErrors.TagNameRequired;
        }

        if (name.Length > TagConstants.MaxNameLength)
        {
            return TagErrors.TagNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            return TagErrors.TagColorRequired;
        }

        if (!Regexes.HexColorRegex.IsMatch(color))
        {
            return TagErrors.TagColorInvalid;
        }

        return new Tag(userId, name, color, createdAt);
    }

    public ErrorOr<Tag> Update(string name, string color, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return TagErrors.TagNameRequired;
        }

        if (name.Length > TagConstants.MaxNameLength)
        {
            return TagErrors.TagNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            return TagErrors.TagColorRequired;
        }

        if (!Regexes.HexColorRegex.IsMatch(color))
        {
            return TagErrors.TagColorInvalid;
        }

        Name = name;
        Color = color;
        UpdatedAt = updatedAt;
        return this;
    }

    public ErrorOr<Tag> Delete(DateTimeOffset deletedAt)
    {
        if (DeletedAt is not null)
        {
            return TagErrors.TagAlreadyDeleted;
        }

        DeletedAt = deletedAt;
        UpdatedAt = deletedAt;
        return this;
    }
}
