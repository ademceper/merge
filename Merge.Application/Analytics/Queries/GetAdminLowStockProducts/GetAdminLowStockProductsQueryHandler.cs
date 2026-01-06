using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;
using AutoMapper;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAdminLowStockProductsQueryHandler : IRequestHandler<GetAdminLowStockProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetAdminLowStockProductsQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetAdminLowStockProductsQueryHandler(
        IDbContext context,
        ILogger<GetAdminLowStockProductsQueryHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAdminLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching admin low stock products. Threshold: {Threshold}", request.Threshold);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= request.Threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}

