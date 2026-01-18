using MediatR;

namespace Merge.Application.ML.Commands.ReviewFraudAlert;

public record ReviewFraudAlertCommand(
    Guid AlertId,
    Guid ReviewedByUserId,
    string Status,
    string? Notes) : IRequest<bool>;
