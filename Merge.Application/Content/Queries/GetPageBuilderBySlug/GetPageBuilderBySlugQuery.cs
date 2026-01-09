using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetPageBuilderBySlug;

public record GetPageBuilderBySlugQuery(
    string Slug,
    bool TrackView = true
) : IRequest<PageBuilderDto?>;

