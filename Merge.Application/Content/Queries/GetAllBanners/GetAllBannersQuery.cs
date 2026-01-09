using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetAllBanners;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllBannersQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BannerDto>>;

