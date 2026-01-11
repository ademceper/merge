using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Queries.GetAllPaymentMethods;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllPaymentMethodsQuery(bool? IsActive = null) : IRequest<IEnumerable<PaymentMethodDto>>;
