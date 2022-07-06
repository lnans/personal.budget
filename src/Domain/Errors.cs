namespace Domain;

public static class Errors
{
    // Common
    public const string AuthFailed = "errors.auth.failed";
    public const string AuthLoginRequired = "errors.auth.login_required";
    public const string AuthPasswordRequired = "errors.auth.password.required";
    public const string UnhandledException = "errors.unhandled_exception";

    // Tags
    public const string TagIdRequired = "errors.tag.id_required";
    public const string TagNameRequired = "errors.tag.name_required";
    public const string TagColorInvalid = "errors.tag.color_invalid";
    public const string TagAlreadyExist = "errors.tag.already_exist";
    public const string TagNotFound = "errors.tag.not_found";

    // Accounts
    public const string AccountIdRequired = "errors.account.id_required";
    public const string AccountNameRequired = "errors.account.name_required";
    public const string AccountBankRequired = "errors.account.bank_required";
    public const string AccountAlreadyExist = "errors.account.already_exist";
    public const string AccountTypeUnknown = "errors.account.type_unknown";
    public const string AccountNotFound = "errors.account.not_found";

    // Transactions
    public const string TransactionRequired = "errors.transaction.required";
    public const string TransactionDescriptionRequired = "errors.transaction.description_required";
    public const string TransactionAccountRequired = "errors.transaction.account_required";
    public const string TransactionTypeUnknown = "errors.transaction.type_unknown";
    public const string TransactionAmountRequired = "errors.transaction.amount_required";
    public const string TransactionCreationDateRequired = "errors.transaction.creation_date_required";
    public const string TransactionExecutionDateRequired = "errors.transaction.execution_date_required";
    public const string TransactionNotFound = "errors.transaction.not_found";
}