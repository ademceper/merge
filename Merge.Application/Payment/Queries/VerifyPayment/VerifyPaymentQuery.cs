using MediatR;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Queries.VerifyPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record VerifyPaymentQuery(string TransactionId) : IRequest<bool>;
