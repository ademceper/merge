using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetLandingPageById;

public record GetLandingPageByIdQuery(
    Guid Id,
    bool TrackView = false
) : IRequest<LandingPageDto?>;

