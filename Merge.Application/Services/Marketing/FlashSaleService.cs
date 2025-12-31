using AutoMapper;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Marketing;


namespace Merge.Application.Services.Marketing;

public class FlashSaleService : IFlashSaleService
{
    private readonly IRepository<FlashSale> _flashSaleRepository;
    private readonly IRepository<FlashSaleProduct> _flashSaleProductRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FlashSaleService> _logger;

    public FlashSaleService(
        IRepository<FlashSale> flashSaleRepository,
        IRepository<FlashSaleProduct> flashSaleProductRepository,
        IRepository<ProductEntity> productRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FlashSaleService> logger)
    {
        _flashSaleRepository = flashSaleRepository;
        _flashSaleProductRepository = flashSaleProductRepository;
        _productRepository = productRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FlashSaleDto?> GetByIdAsync(Guid id)
    {
        var flashSale = await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == id);

        if (flashSale == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // FlashSaleProductDto mapping'i MappingProfile'da yapılmalı
        return _mapper.Map<FlashSaleDto>(flashSale);
    }

    public async Task<IEnumerable<FlashSaleDto>> GetActiveSalesAsync()
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var flashSales = await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .Where(fs => fs.IsActive && fs.StartDate <= now && fs.EndDate >= now)
            .OrderByDescending(fs => fs.StartDate)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FlashSaleDto>>(flashSales);
    }

    public async Task<IEnumerable<FlashSaleDto>> GetAllAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var flashSales = await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .OrderByDescending(fs => fs.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<FlashSaleDto>>(flashSales);
    }

    public async Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            throw new ValidationException("Başlık boş olamaz.");
        }

        if (dto.EndDate <= dto.StartDate)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        var flashSale = _mapper.Map<FlashSale>(dto);
        flashSale = await _flashSaleRepository.AddAsync(flashSale);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !fs.IsDeleted (Global Query Filter)
        var createdFlashSale = await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == flashSale.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FlashSaleDto>(createdFlashSale!);
    }

    public async Task<FlashSaleDto> UpdateAsync(Guid id, UpdateFlashSaleDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.EndDate != null && dto.StartDate != null && dto.EndDate <= dto.StartDate)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        var flashSale = await _flashSaleRepository.GetByIdAsync(id);
        if (flashSale == null)
        {
            throw new NotFoundException("Flash sale", id);
        }

        flashSale.Title = dto.Title;
        flashSale.Description = dto.Description;
        flashSale.StartDate = dto.StartDate;
        flashSale.EndDate = dto.EndDate;
        flashSale.IsActive = dto.IsActive;
        flashSale.BannerImageUrl = dto.BannerImageUrl;

        await _flashSaleRepository.UpdateAsync(flashSale);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !fs.IsDeleted (Global Query Filter)
        var updatedFlashSale = await _context.FlashSales
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FlashSaleDto>(updatedFlashSale!);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var flashSale = await _flashSaleRepository.GetByIdAsync(id);
        if (flashSale == null)
        {
            return false;
        }

        await _flashSaleRepository.DeleteAsync(flashSale);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddProductToSaleAsync(Guid flashSaleId, AddProductToSaleDto dto)
    {
        var flashSale = await _flashSaleRepository.GetByIdAsync(flashSaleId);
        if (flashSale == null)
        {
            throw new NotFoundException("Flash sale", flashSaleId);
        }

        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        // ✅ PERFORMANCE: Removed manual !fsp.IsDeleted (Global Query Filter)
        var existing = await _context.FlashSaleProducts
            .FirstOrDefaultAsync(fsp => fsp.FlashSaleId == flashSaleId && 
                                  fsp.ProductId == dto.ProductId);

        if (existing != null)
        {
            throw new BusinessException("Bu ürün zaten flash sale'de.");
        }

        var flashSaleProduct = new FlashSaleProduct
        {
            FlashSaleId = flashSaleId,
            ProductId = dto.ProductId,
            SalePrice = dto.SalePrice,
            StockLimit = dto.StockLimit,
            SortOrder = dto.SortOrder
        };

        await _flashSaleProductRepository.AddAsync(flashSaleProduct);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProductFromSaleAsync(Guid flashSaleId, Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !fsp.IsDeleted (Global Query Filter)
        var flashSaleProduct = await _context.FlashSaleProducts
            .FirstOrDefaultAsync(fsp => fsp.FlashSaleId == flashSaleId && 
                                  fsp.ProductId == productId);

        if (flashSaleProduct == null)
        {
            return false;
        }

        await _flashSaleProductRepository.DeleteAsync(flashSaleProduct);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

