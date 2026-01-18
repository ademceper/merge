using MediatR;

namespace Merge.Application.Logistics.Commands.DeleteShippingAddress;

public record DeleteShippingAddressCommand(Guid Id) : IRequest<Unit>;

