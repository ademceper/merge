using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogPostBySlug;

public record GetBlogPostBySlugQuery(
    string Slug,
    bool TrackView = true
) : IRequest<BlogPostDto?>;

