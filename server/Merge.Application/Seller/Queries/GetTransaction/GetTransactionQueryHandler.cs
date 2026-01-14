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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, SellerTransactionDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTransactionQueryHandler> _logger;

    public GetTransactionQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetTransactionQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerTransactionDto?> Handle(GetTransactionQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting transaction. TransactionId: {TransactionId}", request.TransactionId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var transaction = await _context.Set<SellerTransaction>()
            .AsNoTracking()
            .Include(t => t.Seller)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return transaction != null ? _mapper.Map<SellerTransactionDto>(transaction) : null;
    }
}
