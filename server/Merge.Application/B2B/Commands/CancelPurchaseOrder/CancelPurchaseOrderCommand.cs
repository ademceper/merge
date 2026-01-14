using MediatR;

namespace Merge.Application.B2B.Commands.CancelPurchaseOrder;

public record CancelPurchaseOrderCommand(Guid Id) : IRequest<bool>;

