using MediatR;

namespace Merge.Application.ML.Commands.ReviewFraudAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReviewFraudAlertCommand(
    Guid AlertId,
    Guid ReviewedByUserId,
    string Status,
    string? Notes) : IRequest<bool>;
