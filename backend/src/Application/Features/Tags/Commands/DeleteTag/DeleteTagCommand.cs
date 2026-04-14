using Application.Interfaces;

namespace Application.Features.Tags.Commands.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : ICommand<DeleteTagResponse>;
