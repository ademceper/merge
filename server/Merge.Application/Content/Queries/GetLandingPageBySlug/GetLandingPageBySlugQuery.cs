using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetLandingPageBySlug;

public record GetLandingPageBySlugQuery(
    string Slug,
    bool TrackView = true
) : IRequest<LandingPageDto?>;

