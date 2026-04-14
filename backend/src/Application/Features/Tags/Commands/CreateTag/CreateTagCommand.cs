using Application.Interfaces;

namespace Application.Features.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(string Name, string Color) : ICommand<CreateTagResponse>;
