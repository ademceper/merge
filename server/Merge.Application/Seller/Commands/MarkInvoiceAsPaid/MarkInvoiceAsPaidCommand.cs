using MediatR;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkInvoiceAsPaidCommand(
    Guid InvoiceId
) : IRequest<bool>;
