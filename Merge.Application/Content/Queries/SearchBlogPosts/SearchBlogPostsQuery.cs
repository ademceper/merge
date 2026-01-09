using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.SearchBlogPosts;

public record SearchBlogPostsQuery(
    string Query,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<BlogPostDto>>;

