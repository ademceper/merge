using MediatR;

namespace Merge.Application.Cart.Commands.NotifyPreOrderAvailable;

public record NotifyPreOrderAvailableCommand(
    Guid PreOrderId) : IRequest;

