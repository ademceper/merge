using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Queries.GetAvailablePaymentMethods;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAvailablePaymentMethodsQuery(decimal OrderAmount) : IRequest<IEnumerable<PaymentMethodDto>>;
