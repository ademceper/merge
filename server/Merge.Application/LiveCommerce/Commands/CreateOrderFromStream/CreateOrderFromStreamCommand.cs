using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateOrderFromStreamCommand(
    Guid StreamId,
    Guid OrderId,
    Guid? ProductId) : IRequest<LiveStreamOrderDto>;

