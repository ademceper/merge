using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using System.Text;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Queries.ExportProductsToCsv;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportProductsToCsvQueryHandler : IRequestHandler<ExportProductsToCsvQuery, byte[]>
{
    private readonly IDbContext _context;
    private readonly ILogger<ExportProductsToCsvQueryHandler> _logger;

    public ExportProductsToCsvQueryHandler(
        IDbContext context,
        ILogger<ExportProductsToCsvQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> Handle(ExportProductsToCsvQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting products to CSV. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        var products = await GetProductsForExportAsync(request.ExportDto, cancellationToken);

        _logger.LogInformation("Products retrieved for CSV export. Count: {Count}", products.Count);

        var csv = new StringBuilder();
        csv.AppendLine("Name,Description,SKU,Price,DiscountPrice,StockQuantity,Brand,Category,ImageUrl,IsActive");

        foreach (var product in products)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("CSV export cancelled. Exported {Count} products so far", products.IndexOf(product));
                break;
            }

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

        _logger.LogInformation("CSV export completed. Total products exported: {Count}", products.Count);

        return Encoding.UTF8.GetBytes(csv.ToString());
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

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
