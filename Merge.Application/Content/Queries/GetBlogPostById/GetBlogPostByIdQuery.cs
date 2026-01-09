using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogPostById;

public record GetBlogPostByIdQuery(
    Guid Id,
    bool TrackView = true
) : IRequest<BlogPostDto?>;

