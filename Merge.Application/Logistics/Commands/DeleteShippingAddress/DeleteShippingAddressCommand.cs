using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteShippingAddress;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteShippingAddressCommand(Guid Id) : IRequest<Unit>;

