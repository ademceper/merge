using MediatR;

namespace Merge.Application.Security.Commands.RejectOrder;

public record RejectOrderCommand(
    Guid VerificationId,
    Guid VerifiedByUserId,
    string Reason
) : IRequest<bool>;
