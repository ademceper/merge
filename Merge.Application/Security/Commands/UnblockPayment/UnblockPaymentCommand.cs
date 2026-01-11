using MediatR;

namespace Merge.Application.Security.Commands.UnblockPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UnblockPaymentCommand(
    Guid CheckId
) : IRequest<bool>;
