using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetAllPickPacks;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
public record GetAllPickPacksQuery(
    PickPackStatus? Status,
    Guid? WarehouseId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PickPackDto>>;

