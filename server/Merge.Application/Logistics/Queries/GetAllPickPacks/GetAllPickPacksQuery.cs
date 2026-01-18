using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetAllPickPacks;

public record GetAllPickPacksQuery(
    PickPackStatus? Status,
    Guid? WarehouseId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PickPackDto>>;

