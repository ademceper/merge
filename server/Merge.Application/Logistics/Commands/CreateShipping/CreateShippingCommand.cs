using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreateShipping;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateShippingCommand(
    Guid OrderId,
    string ShippingProvider,
    decimal ShippingCost) : IRequest<ShippingDto>;

