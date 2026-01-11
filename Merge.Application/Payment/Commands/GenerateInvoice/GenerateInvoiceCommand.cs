using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Commands.GenerateInvoice;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GenerateInvoiceCommand(Guid OrderId) : IRequest<InvoiceDto>;
