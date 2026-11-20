using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Personal.Budget.App.Common;

namespace Personal.Budget.App.Persistence.Entities;

internal sealed class Tag : AppBaseEntity<Tag>
{
    public const int MaxNameLength = 15;
    public const int ColorLength = 7;

    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = null!;

    private Tag() { }

    private Tag(string name, string color, DateTimeOffset createdAt)
        : base(createdAt)
    {
        Name = name;
        Color = color;
    }

    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.Property(t => t.Name).HasMaxLength(MaxNameLength).IsRequired();
        builder.Property(t => t.Color).HasMaxLength(ColorLength).IsRequired();

        base.Configure(builder);
    }

    public static Result<Tag> Create(string? name, string? color, DateTimeOffset createdAt) =>
        Validation
            .Begin()
            .Check(name, "name", ValidName)
            .Check(color, "color", ValidColor)
            .Test(() => new Tag(name!, color!, createdAt));

    public Result<Tag> Rename(string? name) =>
        Validation
            .Begin()
            .Check(name, "name", ValidName)
            .Test(() =>
            {
                Name = name!;
                return this;
            });

    private static Rule<string?> ValidName(Rule<string?> r) =>
        r.NotEmpty(Errors.NameRequired).MaxLength(MaxNameLength, Errors.NameTooLong);

    private static Rule<string?> ValidColor(Rule<string?> r) =>
        r.NotEmpty(Errors.ColorRequired).Matches(Regexes.HexColorRegex, Errors.ColorInvalid);

    private static class Errors
    {
        public static readonly Error NameRequired = Error.Validation("tag.name.required", "Tag name is required.");
        public static readonly Error NameTooLong = Error.Validation(
            "tag.name.too_long",
            $"Tag name must not exceed {MaxNameLength} characters."
        );
        public static readonly Error ColorRequired = Error.Validation("tag.color.required", "Tag color is required.");
        public static readonly Error ColorInvalid = Error.Validation(
            "tag.color.invalid",
            "Tag color must be a valid hexadecimal color (e.g. #FF5733)."
        );
    }
}
