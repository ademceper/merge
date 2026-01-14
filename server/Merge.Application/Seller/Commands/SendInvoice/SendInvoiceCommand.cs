using MediatR;

namespace Merge.Application.Seller.Commands.SendInvoice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SendInvoiceCommand(
    Guid InvoiceId
) : IRequest<bool>;
