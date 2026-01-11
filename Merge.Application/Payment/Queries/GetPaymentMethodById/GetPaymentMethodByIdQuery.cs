using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Queries.GetPaymentMethodById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPaymentMethodByIdQuery(Guid PaymentMethodId) : IRequest<PaymentMethodDto?>;
