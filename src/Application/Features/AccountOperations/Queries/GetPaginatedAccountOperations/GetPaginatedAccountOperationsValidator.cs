using Application.Extensions;
using Application.Models;
using Domain.Accounts;
using FluentValidation;

namespace Application.Features.AccountOperations.Queries.GetPaginatedAccountOperations;

internal sealed class GetPaginatedAccountOperationsValidator : AbstractValidator<GetPaginatedAccountOperationsQuery>
{
    public GetPaginatedAccountOperationsValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThan(0).WithError(PaginationErrors.PageNumberInvalid);

        RuleFor(q => q.PageSize)
            .LessThanOrEqualTo(PaginationConstants.MaxPageSize)
            .WithError(PaginationErrors.PageSizeTooLarge);

        RuleFor(q => q.AccountId)
            .NotEqual(Guid.Empty)
            .WithError(AccountErrors.AccountIdInvalid)
            .When(q => q.AccountId.HasValue);
    }
}
