using MediatR;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.GenerateInvoicePdf;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GenerateInvoicePdfCommand(Guid InvoiceId) : IRequest<string>;
