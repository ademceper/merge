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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerCommissionSettingsQueryHandler : IRequestHandler<GetSellerCommissionSettingsQuery, SellerCommissionSettingsDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerCommissionSettingsQueryHandler> _logger;

    public GetSellerCommissionSettingsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerCommissionSettingsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerCommissionSettingsDto?> Handle(GetSellerCommissionSettingsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller commission settings. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var settings = await _context.Set<SellerCommissionSettings>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return settings != null ? _mapper.Map<SellerCommissionSettingsDto>(settings) : null;
    }
}
