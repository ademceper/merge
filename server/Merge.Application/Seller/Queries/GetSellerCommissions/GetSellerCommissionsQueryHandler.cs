using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerCommissions;

public class GetSellerCommissionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerCommissionsQueryHandler> logger) : IRequestHandler<GetSellerCommissionsQuery, IEnumerable<SellerCommissionDto>>
{

    public async Task<IEnumerable<SellerCommissionDto>> Handle(GetSellerCommissionsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller commissions. SellerId: {SellerId}, Status: {Status}",
            request.SellerId, request.Status?.ToString() ?? "All");

        IQueryable<SellerCommission> query = context.Set<SellerCommission>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .Where(sc => sc.SellerId == request.SellerId);

        if (request.Status.HasValue)
        {
            query = query.Where(sc => sc.Status == request.Status.Value);
        }

        var commissions = await query
            .OrderByDescending(sc => sc.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<SellerCommissionDto>>(commissions);
    }
}
