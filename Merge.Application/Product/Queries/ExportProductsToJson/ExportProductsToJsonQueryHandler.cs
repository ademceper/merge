using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text;
using System.Text.Json;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportProductsToJsonQueryHandler : IRequestHandler<ExportProductsToJsonQuery, byte[]>
{
    private readonly IDbContext _context;
    private readonly ILogger<ExportProductsToJsonQueryHandler> _logger;

    public ExportProductsToJsonQueryHandler(
        IDbContext context,
        ILogger<ExportProductsToJsonQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> Handle(ExportProductsToJsonQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting products to JSON. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        var products = await GetProductsForExportAsync(request.ExportDto, cancellationToken);

        _logger.LogInformation("Products retrieved for JSON export. Count: {Count}", products.Count);

        var exportData = products.Select(p => new
        {
            p.Name,
            p.Description,
            p.SKU,
            p.Price,
            p.DiscountPrice,
            p.StockQuantity,
            p.Brand,
            CategoryName = p.Category.Name,
            p.ImageUrl,
            p.ImageUrls,
            p.IsActive,
            p.Rating,
            p.ReviewCount
        }).ToList();

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        _logger.LogInformation("JSON export completed. Total products exported: {Count}", products.Count);

        return Encoding.UTF8.GetBytes(json);
    }

    private async Task<List<ProductEntity>> GetProductsForExportAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken)
    {
        IQueryable<ProductEntity> query = _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category);

        if (exportDto.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == exportDto.CategoryId.Value);
        }

        if (exportDto.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderBy(p => p.Category.Name)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
