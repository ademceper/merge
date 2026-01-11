using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Queries.GetPaymentById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto?>;
