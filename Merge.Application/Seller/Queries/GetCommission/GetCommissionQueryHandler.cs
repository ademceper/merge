using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Seller.Queries.GetCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCommissionQueryHandler : IRequestHandler<GetCommissionQuery, SellerCommissionDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCommissionQueryHandler> _logger;

    public GetCommissionQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCommissionQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerCommissionDto?> Handle(GetCommissionQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting commission. CommissionId: {CommissionId}", request.CommissionId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        var commission = await _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == request.CommissionId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return commission != null ? _mapper.Map<SellerCommissionDto>(commission) : null;
    }
}
