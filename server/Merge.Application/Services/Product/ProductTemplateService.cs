using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Product;

public class ProductTemplateService : IProductTemplateService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductTemplateService> _logger;

    public ProductTemplateService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductTemplateService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductTemplateDto> CreateTemplateAsync(CreateProductTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product template oluşturuluyor. Name: {Name}, CategoryId: {CategoryId}",
            dto.Name, dto.CategoryId);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId, cancellationToken);

        if (category == null)
        {
            throw new NotFoundException("Kategori", dto.CategoryId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var specificationsJson = dto.Specifications != null ? JsonSerializer.Serialize(dto.Specifications) : null;
        var attributesJson = dto.Attributes != null ? JsonSerializer.Serialize(dto.Attributes) : null;
        
        var template = ProductTemplate.Create(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.DefaultSKUPrefix,
            dto.DefaultPrice,
            dto.DefaultStockQuantity,
            dto.DefaultImageUrl,
            specificationsJson,
            attributesJson,
            dto.IsActive);

        await _context.Set<ProductTemplate>().AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template!.Id, template.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTemplateDto>(template);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductTemplateDto?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<ProductTemplateDto>(template) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductTemplateDto>> GetAllTemplatesAsync(Guid? categoryId = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<ProductTemplate> query = _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var templates = await query
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTemplateDto>>(templates);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTemplateAsync(Guid templateId, UpdateProductTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var specificationsJson = dto.Specifications != null ? JsonSerializer.Serialize(dto.Specifications) : null;
        var attributesJson = dto.Attributes != null ? JsonSerializer.Serialize(dto.Attributes) : null;
        
        template.Update(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            dto.Brand,
            dto.DefaultSKUPrefix,
            dto.DefaultPrice,
            dto.DefaultStockQuantity,
            dto.DefaultImageUrl,
            specificationsJson,
            attributesJson,
            dto.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template == null) return false;

        template.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductDto> CreateProductFromTemplateAsync(CreateProductFromTemplateDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product template'den ürün oluşturuluyor. TemplateId: {TemplateId}, SellerId: {SellerId}",
            dto.TemplateId, dto.SellerId);

        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == dto.TemplateId && t.IsActive, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", dto.TemplateId);
        }

        // Merge template specifications with additional ones
        var templateSpecs = !string.IsNullOrEmpty(template.Specifications)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(template.Specifications) ?? new Dictionary<string, string>()
            : new Dictionary<string, string>();

        if (dto.AdditionalSpecifications != null)
        {
            foreach (var spec in dto.AdditionalSpecifications)
            {
                templateSpecs[spec.Key] = spec.Value;
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
        var productName = !string.IsNullOrEmpty(dto.ProductName) ? dto.ProductName : template.Name;
        var productDescription = !string.IsNullOrEmpty(dto.Description) ? dto.Description : template.Description;
        var sku = new SKU(!string.IsNullOrEmpty(dto.SKU) ? dto.SKU : GenerateSKU(template));
        var price = new Money(dto.Price ?? template.DefaultPrice ?? 0);
        var stockQuantity = dto.StockQuantity ?? template.DefaultStockQuantity ?? 0;
        var brand = template.Brand ?? string.Empty;
        
        var product = ProductEntity.Create(
            productName,
            productDescription,
            sku,
            price,
            stockQuantity,
            template.CategoryId,
            brand,
            dto.SellerId,
            dto.StoreId
        );

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        if (dto.DiscountPrice.HasValue)
        {
            var discountPrice = new Money(dto.DiscountPrice.Value);
            product.SetDiscountPrice(discountPrice);
        }

        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            product.SetImageUrl(dto.ImageUrl);
        }
        else if (!string.IsNullOrEmpty(template.DefaultImageUrl))
        {
            product.SetImageUrl(template.DefaultImageUrl);
        }

        if (dto.ImageUrls != null && dto.ImageUrls.Any())
        {
            product.UpdateImages(product.ImageUrl, dto.ImageUrls.ToList());
        }

        await _context.Set<ProductEntity>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        template.IncrementUsageCount();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product template'den ürün oluşturuldu. ProductId: {ProductId}, TemplateId: {TemplateId}",
            product!.Id, dto.TemplateId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductDto>(product);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductTemplateDto>> GetPopularTemplatesAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var templates = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UsageCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTemplateDto>>(templates);
    }

    private string GenerateSKU(ProductTemplate template)
    {
        var prefix = template.DefaultSKUPrefix ?? "TMP";
        var timestamp = DateTime.UtcNow.Ticks.ToString().Substring(10);
        return $"{prefix}-{timestamp}";
    }

}

