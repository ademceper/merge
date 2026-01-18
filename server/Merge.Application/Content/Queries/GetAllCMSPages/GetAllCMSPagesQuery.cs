using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetAllCMSPages;

public record GetAllCMSPagesQuery(
    string? Status = null,
    bool? ShowInMenu = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CMSPageDto>>;

