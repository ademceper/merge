using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetRecentBlogPosts;

public record GetRecentBlogPostsQuery(
    int Count = 10
) : IRequest<IEnumerable<BlogPostDto>>;

