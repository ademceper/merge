using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetTransaction;

public record GetTransactionQuery(
    Guid TransactionId
) : IRequest<SellerTransactionDto?>;
