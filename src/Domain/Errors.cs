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
    
    // Accounts
    public const string AccountNameRequired = "errors.account.name_required";
    public const string AccountAlreadyExist = "errors.account.already_exist";
    public const string AccountTypeUnknown = "errors.account.type_unknown";
    public const string AccountNotFound = "errors.account.not_found";
    
    // Operations
    public const string OperationDescriptionRequired = "errors.operation.description_required";
    public const string OperationAccountRequired = "errors.operation.account_required";
    public const string OperationTypeUnknown = "errors.operation.type_unknown";
    public const string OperationAmountRequired = "errors.operation.amount_required";
    public const string OperationDateRequired = "errors.operation.date_required";
}