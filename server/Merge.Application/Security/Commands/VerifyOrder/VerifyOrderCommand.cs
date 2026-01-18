using MediatR;

namespace Merge.Application.Security.Commands.VerifyOrder;

public record VerifyOrderCommand(
    Guid VerificationId,
    Guid VerifiedByUserId,
    string? Notes = null
) : IRequest<bool>;
