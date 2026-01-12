using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetPageBuilderBySlug;

public record GetPageBuilderBySlugQuery(
    string Slug,
    bool TrackView = true
) : IRequest<PageBuilderDto?>;

