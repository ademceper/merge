using MediatR;

namespace Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

public record SetDefaultShippingAddressCommand(
    Guid UserId,
    Guid AddressId) : IRequest<Unit>;

