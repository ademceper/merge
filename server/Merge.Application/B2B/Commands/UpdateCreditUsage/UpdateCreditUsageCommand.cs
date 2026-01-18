using MediatR;

namespace Merge.Application.B2B.Commands.UpdateCreditUsage;

public record UpdateCreditUsageCommand(
    Guid CreditTermId,
    decimal Amount
) : IRequest<bool>;

