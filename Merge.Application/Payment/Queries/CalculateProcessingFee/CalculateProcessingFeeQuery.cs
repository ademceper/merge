using MediatR;

namespace Merge.Application.Payment.Queries.CalculateProcessingFee;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CalculateProcessingFeeQuery(Guid PaymentMethodId, decimal Amount) : IRequest<decimal>;
