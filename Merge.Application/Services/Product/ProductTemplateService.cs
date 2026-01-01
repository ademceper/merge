using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Product;

public class ProductTemplateService : IProductTemplateService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductTemplateService> _logger;

    public ProductTemplateService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductTemplateService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductTemplateDto> CreateTemplateAsync(CreateProductTemplateDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

        if (category == null)
        {
            throw new NotFoundException("Kategori", dto.CategoryId);
        }

        var template = new ProductTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            DefaultSKUPrefix = dto.DefaultSKUPrefix,
            DefaultPrice = dto.DefaultPrice,
            DefaultStockQuantity = dto.DefaultStockQuantity,
            DefaultImageUrl = dto.DefaultImageUrl,
            Specifications = dto.Specifications != null ? JsonSerializer.Serialize(dto.Specifications) : null,
            Attributes = dto.Attributes != null ? JsonSerializer.Serialize(dto.Attributes) : null,
            IsActive = dto.IsActive
        };

        await _context.Set<ProductTemplate>().AddAsync(template);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == template.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTemplateDto>(template);
    }

    public async Task<ProductTemplateDto?> GetTemplateByIdAsync(Guid templateId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == templateId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return template != null ? _mapper.Map<ProductTemplateDto>(template) : null;
    }

    public async Task<IEnumerable<ProductTemplateDto>> GetAllTemplatesAsync(Guid? categoryId = null, bool? isActive = null)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTemplateDto>>(templates);
    }

    public async Task<bool> UpdateTemplateAsync(Guid templateId, UpdateProductTemplateDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
        {
            template.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            template.Description = dto.Description;
        }

        if (dto.CategoryId.HasValue)
        {
            template.CategoryId = dto.CategoryId.Value;
        }

        if (dto.Brand != null)
        {
            template.Brand = dto.Brand;
        }

        if (dto.DefaultSKUPrefix != null)
        {
            template.DefaultSKUPrefix = dto.DefaultSKUPrefix;
        }

        if (dto.DefaultPrice.HasValue)
        {
            template.DefaultPrice = dto.DefaultPrice.Value;
        }

        if (dto.DefaultStockQuantity.HasValue)
        {
            template.DefaultStockQuantity = dto.DefaultStockQuantity.Value;
        }

        if (dto.DefaultImageUrl != null)
        {
            template.DefaultImageUrl = dto.DefaultImageUrl;
        }

        if (dto.Specifications != null)
        {
            template.Specifications = JsonSerializer.Serialize(dto.Specifications);
        }

        if (dto.Attributes != null)
        {
            template.Attributes = JsonSerializer.Serialize(dto.Attributes);
        }

        if (dto.IsActive.HasValue)
        {
            template.IsActive = dto.IsActive.Value;
        }

        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null) return false;

        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ProductDto> CreateProductFromTemplateAsync(CreateProductFromTemplateDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == dto.TemplateId && t.IsActive);

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

        // Create product entity directly
        var product = new ProductEntity
        {
            Name = !string.IsNullOrEmpty(dto.ProductName) ? dto.ProductName : template.Name,
            Description = !string.IsNullOrEmpty(dto.Description) ? dto.Description : template.Description,
            SKU = !string.IsNullOrEmpty(dto.SKU) ? dto.SKU : GenerateSKU(template),
            Price = dto.Price ?? template.DefaultPrice ?? 0,
            DiscountPrice = dto.DiscountPrice,
            StockQuantity = dto.StockQuantity ?? template.DefaultStockQuantity ?? 0,
            Brand = template.Brand ?? string.Empty,
            ImageUrl = !string.IsNullOrEmpty(dto.ImageUrl) ? dto.ImageUrl : template.DefaultImageUrl ?? string.Empty,
            ImageUrls = dto.ImageUrls ?? new List<string>(),
            CategoryId = template.CategoryId,
            SellerId = dto.SellerId,
            StoreId = dto.StoreId,
            IsActive = true
        };

        await _context.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Increment template usage count
        template.UsageCount++;
        template.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductTemplateDto>> GetPopularTemplatesAsync(int limit = 10)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var templates = await _context.Set<ProductTemplate>()
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.UsageCount)
            .Take(limit)
            .ToListAsync();

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

