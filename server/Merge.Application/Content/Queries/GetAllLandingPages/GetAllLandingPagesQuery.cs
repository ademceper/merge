using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetAllLandingPages;

public record GetAllLandingPagesQuery(
    string? Status = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<LandingPageDto>>;

