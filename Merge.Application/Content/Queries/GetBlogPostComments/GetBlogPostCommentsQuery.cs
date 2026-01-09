using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogPostComments;

public record GetBlogPostCommentsQuery(
    Guid PostId,
    bool? IsApproved = true,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BlogCommentDto>>;

