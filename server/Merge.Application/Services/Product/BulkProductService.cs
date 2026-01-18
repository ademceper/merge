using AutoMapper;
using System.Globalization;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;


namespace Merge.Application.Services.Product;

public class BulkProductService(
    IRepository productRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<BulkProductService> logger) : IBulkProductService
{

    public async Task<BulkProductImportResultDto> ImportProductsFromCsvAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("CSV bulk import başlatıldı");

        List<string> errors = [];
        List<ProductDto> importedProducts = [];
        var totalProcessed = 0;
        var successCount = 0;
        var failureCount = 0;

        var reader = new StreamReader(fileStream);

        // Skip header line
        await reader.ReadLineAsync();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            totalProcessed++;

            try
            {
                var values = ParseCsvLine(line);
                if (values.Length < 8)
                {
                    failureCount++;
                    errors.Add($"Line {totalProcessed}: Insufficient columns");
                    continue;
                }

                var productDto = new BulkProductImportDto(
                    Name: values[0],
                    Description: values[1],
                    SKU: values[2],
                    Price: decimal.Parse(values[3], CultureInfo.InvariantCulture),
                    DiscountPrice: string.IsNullOrWhiteSpace(values[4]) ? null : decimal.Parse(values[4], CultureInfo.InvariantCulture),
                    StockQuantity: int.Parse(values[5]),
                    Brand: values[6],
                    CategoryName: values[7],
                    ImageUrl: values.Length > 8 ? values[8] : string.Empty,
                    IsActive: true);

                var product = await ImportSingleProductAsync(productDto, cancellationToken);
                if (product is not null)
                {
                    successCount++;
                    var importedProductDto = mapper.Map<ProductDto>(product);
                    importedProducts.Add(importedProductDto);
                }
                else
                {
                    failureCount++;
                    errors.Add($"Line {totalProcessed}: Failed to import product '{productDto.Name}'");
                }
            }
            catch (Exception ex)
            {
                failureCount++;
                errors.Add($"Line {totalProcessed}: {ex.Message}");
                    logger.LogWarning(ex, "CSV import hatası. Line: {Line}", totalProcessed);
            }
        }

        logger.LogInformation(
            "CSV bulk import tamamlandı. TotalProcessed: {TotalProcessed}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}",
            totalProcessed, successCount, failureCount);

        return new BulkProductImportResultDto(
            TotalProcessed: totalProcessed,
            SuccessCount: successCount,
            FailureCount: failureCount,
            Errors: errors,
            ImportedProducts: importedProducts);
    }

    public async Task<BulkProductImportResultDto> ImportProductsFromJsonAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("JSON bulk import başlatıldı");

        List<string> errors = [];
        List<ProductDto> importedProducts = [];
        var totalProcessed = 0;
        var successCount = 0;
        var failureCount = 0;

        try
        {
            var products = await JsonSerializer.DeserializeAsync<List<BulkProductImportDto>>(fileStream, cancellationToken: cancellationToken);

            if (products is null || products.Count == 0)
            {
                errors.Add("No products found in JSON file");
                return new BulkProductImportResultDto(
                    TotalProcessed: totalProcessed,
                    SuccessCount: successCount,
                    FailureCount: failureCount,
                    Errors: errors,
                    ImportedProducts: importedProducts);
            }

            foreach (var productDto in products)
            {
                if (cancellationToken.IsCancellationRequested) break;

                totalProcessed++;

                try
                {
                    var product = await ImportSingleProductAsync(productDto, cancellationToken);
                    if (product is not null)
                    {
                        successCount++;
                        var importedProductDto = mapper.Map<ProductDto>(product);
                        importedProducts.Add(importedProductDto);
                    }
                    else
                    {
                        failureCount++;
                        errors.Add($"Failed to import product '{productDto.Name}'");
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    errors.Add($"Product '{productDto.Name}': {ex.Message}");
                    logger.LogWarning(ex, "JSON import hatası. Product: {ProductName}", productDto.Name);
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"JSON parsing error: {ex.Message}");
            logger.LogError(ex, "JSON parsing hatası");
        }

        logger.LogInformation(
            "JSON bulk import tamamlandı. TotalProcessed: {TotalProcessed}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}",
            totalProcessed, successCount, failureCount);

        return new BulkProductImportResultDto(
            TotalProcessed: totalProcessed,
            SuccessCount: successCount,
            FailureCount: failureCount,
            Errors: errors,
            ImportedProducts: importedProducts);
    }

    public async Task<byte[]> ExportProductsToCsvAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default)
    {
        var products = await GetProductsForExportAsync(exportDto, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Name,Description,SKU,Price,DiscountPrice,StockQuantity,Brand,Category,ImageUrl,IsActive");

        foreach (var product in products)
        {
            if (cancellationToken.IsCancellationRequested) break;

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

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportProductsToJsonAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default)
    {
        var products = await GetProductsForExportAsync(exportDto, cancellationToken);

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

        return Encoding.UTF8.GetBytes(json);
    }

    public async Task<byte[]> ExportProductsToExcelAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default)
    {
        // For Excel export, we'll use CSV format as a simple alternative
        // In production, use EPPlus or ClosedXML library for real Excel files
        return await ExportProductsToCsvAsync(exportDto, cancellationToken);
    }

    // Helper methods

    private async Task<ProductEntity?> ImportSingleProductAsync(BulkProductImportDto dto, CancellationToken cancellationToken = default)
    {
        // Check if SKU already exists
        var existingProduct = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU, cancellationToken);

        if (existingProduct != null)
        {
            throw new BusinessException($"Bu SKU ile ürün zaten mevcut: '{dto.SKU}'");
        }

        // Find category by name
        var category = await context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == dto.CategoryName, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Kategori", Guid.Empty);
        }

        var sku = new SKU(dto.SKU);
        var price = new Money(dto.Price);
        var product = ProductEntity.Create(
            dto.Name,
            dto.Description,
            sku,
            price,
            dto.StockQuantity,
            category.Id,
            dto.Brand,
            null, // sellerId
            null  // storeId
        );

        if (dto.DiscountPrice.HasValue)
        {
            var discountPrice = new Money(dto.DiscountPrice.Value);
            product.SetDiscountPrice(discountPrice);
        }

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            product.SetImageUrl(dto.ImageUrl);
        }

        if (!dto.IsActive)
        {
            product.Deactivate();
        }

        product = await productRepository.AddAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return product;
    }

    private async Task<List<ProductEntity>> GetProductsForExportAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default)
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

    private string[] ParseCsvLine(string line)
    {
        List<string> values = [];
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString().Trim());
        return values.ToArray();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("\"", "\"\"");
    }
}
