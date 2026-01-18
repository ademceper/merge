using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetReturnRequestById;

public record GetReturnRequestByIdQuery(
    Guid ReturnRequestId
) : IRequest<ReturnRequestDto?>;
