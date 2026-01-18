using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.NotifyPreOrderAvailable;

public record NotifyPreOrderAvailableCommand(
    Guid PreOrderId) : IRequest;

