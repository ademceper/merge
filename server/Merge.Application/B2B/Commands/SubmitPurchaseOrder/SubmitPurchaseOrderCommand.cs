using MediatR;

namespace Merge.Application.B2B.Commands.SubmitPurchaseOrder;

public record SubmitPurchaseOrderCommand(Guid Id) : IRequest<bool>;

