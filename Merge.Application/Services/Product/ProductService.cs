using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Product;
using Microsoft.Extensions.Logging;


namespace Merge.Application.Services.Product;

public class ProductService : IProductService
{
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IRepository<ProductEntity> productRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed !p.IsDeleted (Global Query Filter handles it)
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + removed manual !p.IsDeleted check
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + removed manual !p.IsDeleted check
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved products by category. CategoryId: {CategoryId}, Count: {Count}",
            categoryId, products.Count);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            throw new ValidationException("Arama terimi boş olamaz.");
        }

        if (page < 1)
        {
            throw new ValidationException("Sayfa numarası 1'den küçük olamaz.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
        }

        // ✅ PERFORMANCE: EF.Functions.ILike for case-insensitive search with PostgreSQL
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed !p.IsDeleted (Global Query Filter)
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive &&
                (EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                 EF.Functions.ILike(p.Description, $"%{searchTerm}%") ||
                 EF.Functions.ILike(p.Brand, $"%{searchTerm}%")))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation(
            "Product search completed. Term: {SearchTerm}, Results: {Count}",
            searchTerm, products.Count);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto> CreateAsync(ProductDto productDto)
    {
        if (productDto == null)
        {
            throw new ArgumentNullException(nameof(productDto));
        }

        if (string.IsNullOrWhiteSpace(productDto.Name))
        {
            throw new ValidationException("Ürün adı boş olamaz.");
        }

        if (productDto.Price < 0)
        {
            throw new ValidationException("Ürün fiyatı negatif olamaz.");
        }

        var product = _mapper.Map<ProductEntity>(productDto);
        product = await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, ProductDto productDto)
    {
        if (productDto == null)
        {
            throw new ArgumentNullException(nameof(productDto));
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException("Ürün", id);
        }

        if (string.IsNullOrWhiteSpace(productDto.Name))
        {
            throw new ValidationException("Ürün adı boş olamaz.");
        }

        if (productDto.Price < 0)
        {
            throw new ValidationException("Ürün fiyatı negatif olamaz.");
        }

        if (productDto.StockQuantity < 0)
        {
            throw new ValidationException("Stok miktarı negatif olamaz.");
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.DiscountPrice = productDto.DiscountPrice;
        product.StockQuantity = productDto.StockQuantity;
        product.Brand = productDto.Brand;
        product.ImageUrl = productDto.ImageUrl;
        product.ImageUrls = productDto.ImageUrls;
        product.IsActive = productDto.IsActive;
        product.CategoryId = productDto.CategoryId;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(product);
        return true;
    }
}

