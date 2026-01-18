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

namespace Merge.Application.Seller.Queries.GetSellerCommissionSettings;

public class GetSellerCommissionSettingsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerCommissionSettingsQueryHandler> logger) : IRequestHandler<GetSellerCommissionSettingsQuery, SellerCommissionSettingsDto?>
{

    public async Task<SellerCommissionSettingsDto?> Handle(GetSellerCommissionSettingsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller commission settings. SellerId: {SellerId}", request.SellerId);

        var settings = await context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        return settings is not null ? mapper.Map<SellerCommissionSettingsDto>(settings) : null;
    }
}
