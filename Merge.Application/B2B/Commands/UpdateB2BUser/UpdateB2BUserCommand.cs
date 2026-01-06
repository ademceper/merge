using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateB2BUserCommand(
    Guid Id,
    UpdateB2BUserDto Dto
) : IRequest<bool>;

