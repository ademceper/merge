using MediatR;

namespace Merge.Application.Content.Commands.DeleteSEOSettings;

public record DeleteSEOSettingsCommand(
    string PageType,
    Guid EntityId
) : IRequest<bool>;

