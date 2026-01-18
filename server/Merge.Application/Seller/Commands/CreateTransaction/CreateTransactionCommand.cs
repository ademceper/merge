using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.CreateTransaction;

public record CreateTransactionCommand(
    Guid SellerId,
    SellerTransactionType TransactionType,
    decimal Amount,
    string Description,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null
) : IRequest<SellerTransactionDto>;
