using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetTransaction;

public class GetTransactionQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTransactionQueryHandler> logger) : IRequestHandler<GetTransactionQuery, SellerTransactionDto?>
{

    public async Task<SellerTransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting transaction. TransactionId: {TransactionId}", request.TransactionId);

        var transaction = await context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        return transaction is not null ? mapper.Map<SellerTransactionDto>(transaction) : null;
    }
}
