using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetFeaturedBlogPosts;

public record GetFeaturedBlogPostsQuery(
    int Count = 5
) : IRequest<IEnumerable<BlogPostDto>>;

