using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.PatchCreditTerm;

public record PatchCreditTermCommand(
    Guid Id,
    PatchCreditTermDto PatchDto
) : IRequest<bool>;
