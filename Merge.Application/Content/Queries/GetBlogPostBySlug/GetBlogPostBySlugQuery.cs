using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetBlogPostBySlug;

public record GetBlogPostBySlugQuery(
    string Slug,
    bool TrackView = true
) : IRequest<BlogPostDto?>;

