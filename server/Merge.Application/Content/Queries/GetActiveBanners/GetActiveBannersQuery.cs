using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Queries.GetActiveBanners;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetActiveBannersQuery(
    string? Position = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BannerDto>>;

