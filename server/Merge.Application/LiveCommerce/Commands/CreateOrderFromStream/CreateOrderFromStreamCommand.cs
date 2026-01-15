using MediatR;
using Merge.Application.DTOs.LiveCommerce;

namespace Merge.Application.LiveCommerce.Commands.CreateOrderFromStream;

public record CreateOrderFromStreamCommand(
    Guid StreamId,
    Guid OrderId,
    Guid? ProductId) : IRequest<LiveStreamOrderDto>;
