using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Commands.CreatePreOrder;

public record CreatePreOrderCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    string? VariantOptions,
    string? Notes) : IRequest<PreOrderDto>;

