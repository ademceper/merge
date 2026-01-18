using MediatR;

namespace Merge.Application.Security.Commands.UnblockPayment;

public record UnblockPaymentCommand(
    Guid CheckId
) : IRequest<bool>;
