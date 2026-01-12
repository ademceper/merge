using MediatR;
using Microsoft.EntityFrameworkCore;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ImportProductsFromCsvCommandHandler : IRequestHandler<ImportProductsFromCsvCommand, BulkProductImportResultDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<ImportProductsFromCsvCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";
    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";
    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";
    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public ImportProductsFromCsvCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<ImportProductsFromCsvCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BulkProductImportResultDto> Handle(ImportProductsFromCsvCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CSV bulk import başlatıldı");

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var errors = new List<string>();
        var importedProducts = new List<ProductDto>();
        int totalProcessed = 0;
        int successCount = 0;
        int failureCount = 0;
        var reader = new StreamReader(request.FileStream);

        // Skip header line
        await reader.ReadLineAsync();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
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

                    // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
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
                    if (product != null)
                    {
                        successCount++;
                        var importedProductDto = _mapper.Map<ProductDto>(product);
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
                    _logger.LogWarning(ex, "CSV import hatası. Line: {Line}", totalProcessed);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation - Bulk import sonrası tüm product cache'lerini invalidate et
            // Note: Pattern-based invalidation gerektirir. Şimdilik tüm product cache'lerini invalidate ediyoruz
            await _cache.RemoveAsync(CACHE_KEY_ALL_PRODUCTS_PAGED, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_PRODUCTS_SEARCH, cancellationToken);
            // Category-based cache'ler pattern-based invalidation gerektirir, şimdilik expiration'a güveniyoruz

            _logger.LogInformation(
                "CSV bulk import tamamlandı. TotalProcessed: {TotalProcessed}, SuccessCount: {SuccessCount}, FailureCount: {FailureCount}",
                totalProcessed, successCount, failureCount);

            // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
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
            _logger.LogError(ex, "Error during CSV bulk import");
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductEntity?> ImportSingleProductAsync(BulkProductImportDto dto, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var existingProduct = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU, cancellationToken);

        if (existingProduct != null)
        {
            _logger.LogWarning("SKU already exists: {SKU}", dto.SKU);
            return null;
        }

        var category = await _context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == dto.CategoryName, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryName}", dto.CategoryName);
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

        await _context.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product;
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
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
