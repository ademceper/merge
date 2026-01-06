using MediatR;

namespace Merge.Application.B2B.Commands.UpdateCreditUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCreditUsageCommand(
    Guid CreditTermId,
    decimal Amount
) : IRequest<bool>;

