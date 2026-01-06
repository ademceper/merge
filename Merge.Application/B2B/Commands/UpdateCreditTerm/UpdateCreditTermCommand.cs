using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCreditTermCommand(
    Guid Id,
    CreateCreditTermDto Dto
) : IRequest<bool>;

