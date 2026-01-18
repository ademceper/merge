using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ProcessExpiredPreOrders;

public record ProcessExpiredPreOrdersCommand : IRequest;

