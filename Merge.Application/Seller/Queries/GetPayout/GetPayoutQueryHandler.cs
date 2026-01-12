using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPayoutQueryHandler : IRequestHandler<GetPayoutQuery, CommissionPayoutDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPayoutQueryHandler> _logger;

    public GetPayoutQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPayoutQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CommissionPayoutDto?> Handle(GetPayoutQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting payout. PayoutId: {PayoutId}", request.PayoutId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var payout = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PayoutId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return payout != null ? _mapper.Map<CommissionPayoutDto>(payout) : null;
    }
}
