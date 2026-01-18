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

namespace Merge.Application.Seller.Queries.GetCommission;

public class GetCommissionQueryHandler(IDbContext context, IMapper mapper, ILogger<GetCommissionQueryHandler> logger) : IRequestHandler<GetCommissionQuery, SellerCommissionDto?>
{

    public async Task<SellerCommissionDto?> Handle(GetCommissionQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting commission. CommissionId: {CommissionId}", request.CommissionId);

        var commission = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Include(sc => sc.Seller)
            .Include(sc => sc.Order)
            .Include(sc => sc.OrderItem)
            .FirstOrDefaultAsync(sc => sc.Id == request.CommissionId, cancellationToken);

        return commission != null ? mapper.Map<SellerCommissionDto>(commission) : null;
    }
}
