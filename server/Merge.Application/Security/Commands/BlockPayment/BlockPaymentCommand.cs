using MediatR;

namespace Merge.Application.Security.Commands.BlockPayment;

public record BlockPaymentCommand(
    Guid CheckId,
    string Reason
) : IRequest<bool>;
