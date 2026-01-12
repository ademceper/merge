using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrderStatistics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetOrderStatisticsQuery(
    Guid UserId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<OrderStatisticsDto>;
