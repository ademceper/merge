using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Queries.GetInvoiceById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<InvoiceDto?>;
