using ErrorOr;

namespace Domain.Tags;

public static class TagErrors
{
    public static Error TagNameRequired =>
        Error.Validation(code: "Tag.Name.Required", description: "Tag name is required.");

    public static Error TagNameTooLong =>
        Error.Validation(
            code: "Tag.Name.TooLong",
            description: $"Tag name must not exceed {TagConstants.MaxNameLength} characters."
        );

    public static Error TagColorRequired =>
        Error.Validation(code: "Tag.Color.Required", description: "Tag color is required.");

    public static Error TagColorInvalid =>
        Error.Validation(
            code: "Tag.Color.Invalid",
            description: "Tag color must be a valid hexadecimal color (e.g. #FF5733)."
        );

    public static Error TagNotFound => Error.NotFound(code: "Tag.NotFound", description: "Tag not found.");

    public static Error TagAlreadyDeleted =>
        Error.Validation(code: "Tag.AlreadyDeleted", description: "Tag is already deleted.");

    public static Error TagIsLinkedToOperation =>
        Error.Validation(code: "Tag.IsLinkedToOperation", description: "Tag is linked to an account operation.");

    public static Error TagDuplicated =>
        Error.Validation(code: "Tag.Duplicated", description: "Tag ids must be unique.");
}
