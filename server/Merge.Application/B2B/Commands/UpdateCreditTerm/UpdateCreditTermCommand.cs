using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateCreditTerm;

public record UpdateCreditTermCommand(
    Guid Id,
    CreateCreditTermDto Dto
) : IRequest<bool>;

