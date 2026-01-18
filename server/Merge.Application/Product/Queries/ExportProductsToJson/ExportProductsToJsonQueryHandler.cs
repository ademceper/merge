using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text;
using System.Text.Json;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

public class ExportProductsToJsonQueryHandler(IDbContext context, ILogger<ExportProductsToJsonQueryHandler> logger) : IRequestHandler<ExportProductsToJsonQuery, byte[]>
{

    public async Task<byte[]> Handle(ExportProductsToJsonQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Exporting products to JSON. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        var products = await GetProductsForExportAsync(request.ExportDto, cancellationToken);

        logger.LogInformation("Products retrieved for JSON export. Count: {Count}", products.Count);

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

        logger.LogInformation("JSON export completed. Total products exported: {Count}", products.Count);

        return Encoding.UTF8.GetBytes(json);
    }

    private async Task<List<ProductEntity>> GetProductsForExportAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken)
    {
        IQueryable<ProductEntity> query = context.Set<ProductEntity>()
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
