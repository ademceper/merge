using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.CreatePreOrder;

public record CreatePreOrderCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    string? VariantOptions,
    string? Notes) : IRequest<PreOrderDto>;

