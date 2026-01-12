using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CreateReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateReturnRequestCommand(
    CreateReturnRequestDto Dto
) : IRequest<ReturnRequestDto>;
