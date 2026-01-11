using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerCommissions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerCommissionsQueryHandler : IRequestHandler<GetSellerCommissionsQuery, IEnumerable<SellerCommissionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerCommissionsQueryHandler> _logger;

    public GetSellerCommissionsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerCommissionsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SellerCommissionDto>> Handle(GetSellerCommissionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller commissions. SellerId: {SellerId}, Status: {Status}",
            request.SellerId, request.Status ?? "All");

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sc.IsDeleted (Global Query Filter)
        IQueryable<SellerCommission> query = _context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => sc.SellerId == request.SellerId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var commissionStatus = Enum.Parse<CommissionStatus>(request.Status, true);
            query = query.Where(sc => sc.Status == commissionStatus);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SellerCommissionDto>>(commissions);
    }
}
