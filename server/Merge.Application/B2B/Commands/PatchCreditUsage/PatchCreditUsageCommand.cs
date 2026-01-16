using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.PatchCreditUsage;

public record PatchCreditUsageCommand(
    Guid CreditTermId,
    PatchCreditUsageDto PatchDto
) : IRequest<bool>;
