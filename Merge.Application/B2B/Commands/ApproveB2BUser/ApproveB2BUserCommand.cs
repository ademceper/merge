using MediatR;

namespace Merge.Application.B2B.Commands.ApproveB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveB2BUserCommand(
    Guid Id,
    Guid ApprovedByUserId
) : IRequest<bool>;

