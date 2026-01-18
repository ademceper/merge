using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Globalization;
using System.Text;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.ImportProductsFromCsv;

public class ImportProductsFromCsvCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ImportProductsFromCsvCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<ImportProductsFromCsvCommand, BulkProductImportResultDto>
{

    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<BulkProductImportResultDto> Handle(ImportProductsFromCsvCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("CSV bulk import başlatıldı");

        List<string> errors = [];
        List<ProductDto> importedProducts = [];
        int totalProcessed = 0;
        int successCount = 0;
        int failureCount = 0;
        var reader = new StreamReader(request.FileStream);

        // Skip header line
        await reader.ReadLineAsync();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
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
                        ImageUrl: values.Length > 8 ? values[8] : string.Empty,
                        CategoryName: values[7],
                        IsActive: true
                    );

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

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Note: Pattern-based invalidation gerektirir. Şimdilik tüm product cache'lerini invalidate ediyoruz
            await cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);
            // Category-based cache'ler pattern-based invalidation gerektirir, şimdilik expiration'a güveniyoruz

            logger.LogInformation(
                "CSV bulk import tamamlandı. TotalProcessed: {TotalProcessed}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}",
                totalProcessed, successCount, failureCount);

            return new BulkProductImportResultDto(
                TotalProcessed: totalProcessed,
                SuccessCount: successCount,
                FailureCount: failureCount,
                Errors: errors.AsReadOnly(),
                ImportedProducts: importedProducts.AsReadOnly()
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during CSV bulk import");
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductEntity?> ImportSingleProductAsync(BulkProductImportDto dto, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var existingProduct = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU, cancellationToken);

        if (existingProduct is not null)
        {
            logger.LogWarning("SKU already exists: {SKU}", dto.SKU);
            return null;
        }

        var category = await context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == dto.CategoryName, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Category not found: {CategoryName}", dto.CategoryName);
            return null;
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
            dto.Brand
        );

        if (dto.DiscountPrice.HasValue)
        {
            product.SetDiscountPrice(new Money(dto.DiscountPrice.Value));
        }

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            product.SetImageUrl(dto.ImageUrl);
        }

        if (!dto.IsActive)
        {
            product.Deactivate();
        }

        await context.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }

    private string[] ParseCsvLine(string line)
    {
        List<string> values = [];
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        values.Add(current.ToString());

        return values.ToArray();
    }
}
