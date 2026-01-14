using MediatR;

namespace Merge.Application.Security.Commands.BlockPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record BlockPaymentCommand(
    Guid CheckId,
    string Reason
) : IRequest<bool>;
