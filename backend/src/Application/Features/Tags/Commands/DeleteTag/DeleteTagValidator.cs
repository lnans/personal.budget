using FluentValidation;

namespace Application.Features.Tags.Commands.DeleteTag;

internal sealed class DeleteTagValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagValidator()
    {
        RuleFor(q => q.Id).NotEmpty();
    }
}
