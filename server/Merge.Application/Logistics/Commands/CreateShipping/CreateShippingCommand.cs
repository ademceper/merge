using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.CreateShipping;

public record CreateShippingCommand(
    Guid OrderId,
    string ShippingProvider,
    decimal ShippingCost) : IRequest<ShippingDto>;

