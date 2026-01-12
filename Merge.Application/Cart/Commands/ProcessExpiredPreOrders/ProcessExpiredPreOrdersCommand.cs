using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ProcessExpiredPreOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ProcessExpiredPreOrdersCommand : IRequest;

