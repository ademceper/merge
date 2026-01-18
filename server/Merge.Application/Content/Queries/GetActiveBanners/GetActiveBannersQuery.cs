using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetActiveBanners;

public record GetActiveBannersQuery(
    string? Position = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BannerDto>>;

