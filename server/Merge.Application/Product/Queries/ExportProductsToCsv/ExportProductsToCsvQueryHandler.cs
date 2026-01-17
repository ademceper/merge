using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.ExportProductsToCsv;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportProductsToCsvQueryHandler(IDbContext context, ILogger<ExportProductsToCsvQueryHandler> logger) : IRequestHandler<ExportProductsToCsvQuery, byte[]>
{

    public async Task<byte[]> Handle(ExportProductsToCsvQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Exporting products to CSV. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        var products = await GetProductsForExportAsync(request.ExportDto, cancellationToken);

        logger.LogInformation("Products retrieved for CSV export. Count: {Count}", products.Count);

        var csv = new StringBuilder();
        csv.AppendLine("Name,Description,SKU,Price,DiscountPrice,StockQuantity,Brand,Category,ImageUrl,IsActive");

        // ✅ PERFORMANCE FIX: IndexOf() O(n) yerine for loop ile O(1) index erişimi
        for (var i = 0; i < products.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("CSV export cancelled. Exported {Count} products so far", i);
                break;
            }

            var product = products[i];
            csv.AppendLine($"\"{EscapeCsv(product.Name)}\"," +
                          $"\"{EscapeCsv(product.Description)}\"," +
                          $"\"{product.SKU}\"," +
                          $"{product.Price}," +
                          $"{product.DiscountPrice?.ToString() ?? ""}," +
                          $"{product.StockQuantity}," +
                          $"\"{EscapeCsv(product.Brand)}\"," +
                          $"\"{EscapeCsv(product.Category.Name)}\"," +
                          $"\"{product.ImageUrl}\"," +
                          $"{product.IsActive}");
        }

        logger.LogInformation("CSV export completed. Total products exported: {Count}", products.Count);

        return Encoding.UTF8.GetBytes(csv.ToString());
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

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
