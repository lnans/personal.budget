using Application.Interfaces;

namespace Application.Features.Tags.Commands.UpdateTag;

public sealed record UpdateTagCommand(Guid Id, string Name, string Color) : ICommand<UpdateTagResponse>;
