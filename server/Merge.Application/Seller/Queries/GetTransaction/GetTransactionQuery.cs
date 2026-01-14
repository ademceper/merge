using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetTransaction;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTransactionQuery(
    Guid TransactionId
) : IRequest<SellerTransactionDto?>;
