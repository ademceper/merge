using MediatR;

namespace Merge.Application.Content.Commands.DeleteSitemapEntry;

public record DeleteSitemapEntryCommand(
    Guid Id
) : IRequest<bool>;

