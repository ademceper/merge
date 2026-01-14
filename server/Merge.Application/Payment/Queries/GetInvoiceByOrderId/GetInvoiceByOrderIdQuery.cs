using MediatR;
using Merge.Application.DTOs.Payment;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Queries.GetInvoiceByOrderId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInvoiceByOrderIdQuery(Guid OrderId) : IRequest<InvoiceDto?>;
