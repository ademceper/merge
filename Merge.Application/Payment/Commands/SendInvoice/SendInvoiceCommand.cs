using MediatR;

namespace Merge.Application.Payment.Commands.SendInvoice;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SendInvoiceCommand(Guid InvoiceId) : IRequest<bool>;
