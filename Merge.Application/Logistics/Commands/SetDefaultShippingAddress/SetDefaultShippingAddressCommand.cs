using MediatR;

namespace Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetDefaultShippingAddressCommand(
    Guid UserId,
    Guid AddressId) : IRequest<Unit>;

