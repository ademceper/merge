using MediatR;

namespace Merge.Application.Content.Commands.UpdateSitemapEntry;

public record UpdateSitemapEntryCommand(
    Guid Id,
    string? Url = null,
    string? ChangeFrequency = null,
    decimal? Priority = null
) : IRequest<bool>;

