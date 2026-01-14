using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogPosts;

public record GetBlogPostsQuery(
    Guid? CategoryId = null,
    string? Status = "Published",
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<BlogPostDto>>;

