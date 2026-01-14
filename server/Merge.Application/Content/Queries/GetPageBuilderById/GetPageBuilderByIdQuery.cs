using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetPageBuilderById;

public record GetPageBuilderByIdQuery(
    Guid Id,
    bool TrackView = false
) : IRequest<PageBuilderDto?>;

