using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetReturnRequestById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReturnRequestByIdQuery(
    Guid ReturnRequestId
) : IRequest<ReturnRequestDto?>;
