namespace Domain;

public static class Errors
{
    // Common
    public const string AuthFailed = "errors.auth_failed";
    public const string UnhandledException = "errors.unhandled_exception";
    
    // Operation Tags
    public const string OperationTagNameRequired = "errors.operation_tag.name_required";
    public const string OperationTagColorInvalid = "errors.operation_tag.color_invalid";
    public const string OperationTagAlreadyExist = "errors.operation_tag.already_exist";
    public const string OperationTagNotFound = "errors.operation_tag.not_found";
}