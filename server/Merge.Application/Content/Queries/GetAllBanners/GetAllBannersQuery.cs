using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetAllBanners;

public record GetAllBannersQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BannerDto>>;

