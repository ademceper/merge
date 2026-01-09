using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;


namespace Merge.Application.Services.Content;

public class BannerService : IBannerService
{
    private readonly IRepository<Banner> _bannerRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BannerService> _logger;

    public BannerService(
        IRepository<Banner> bannerRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<BannerService> logger)
    {
        _bannerRepository = bannerRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<BannerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Direct DbContext query for better control
        var banner = await _context.Set<Banner>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        return banner == null ? null : _mapper.Map<BannerDto>(banner);
    }

    public async Task<IEnumerable<BannerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var banners = await _context.Set<Banner>()
            .AsNoTracking()
            .OrderBy(b => b.Position)
            .ThenBy(b => b.SortOrder)
            .Take(500) // ✅ Güvenlik: Maksimum 500 banner
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<BannerDto>(banners.Count);
        foreach (var banner in banners)
        {
            result.Add(_mapper.Map<BannerDto>(banner));
        }
        return result;
    }

    public async Task<PagedResult<BannerDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Banner>()
            .AsNoTracking()
            .OrderBy(b => b.Position)
            .ThenBy(b => b.SortOrder);

        var totalCount = await query.CountAsync(cancellationToken);
        var banners = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BannerDto>
        {
            Items = _mapper.Map<List<BannerDto>>(banners),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<BannerDto>> GetActiveBannersAsync(string? position = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var now = DateTime.UtcNow;
        var query = _context.Set<Banner>()
            .AsNoTracking()
            .Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now));

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        var banners = await query
            .OrderBy(b => b.SortOrder)
            .Take(200) // ✅ Güvenlik: Maksimum 200 aktif banner
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<BannerDto>(banners.Count);
        foreach (var banner in banners)
        {
            result.Add(_mapper.Map<BannerDto>(banner));
        }
        return result;
    }

    public async Task<PagedResult<BannerDto>> GetActiveBannersAsync(string? position, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.Set<Banner>()
            .AsNoTracking()
            .Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now));

        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(b => b.Position == position);
        }

        var orderedQuery = query.OrderBy(b => b.SortOrder);
        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var banners = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BannerDto>
        {
            Items = _mapper.Map<List<BannerDto>>(banners),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("Banner olusturuluyor. Title: {Title}, Position: {Position}", dto.Title, dto.Position);

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var banner = Banner.Create(
                dto.Title,
                dto.ImageUrl,
                dto.Position,
                dto.Description,
                dto.LinkUrl,
                dto.SortOrder,
                true, // Default IsActive = true
                dto.StartDate,
                dto.EndDate,
                dto.CategoryId,
                dto.ProductId);
            
            banner = await _bannerRepository.AddAsync(banner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Banner olusturuldu. BannerId: {BannerId}, Title: {Title}", banner.Id, banner.Title);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner olusturma hatasi. Title: {Title}, Position: {Position}", dto.Title, dto.Position);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<BannerDto> UpdateAsync(Guid id, UpdateBannerDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("Banner guncelleniyor. BannerId: {BannerId}, Title: {Title}", id, dto.Title);

        try
        {
            var banner = await _bannerRepository.GetByIdAsync(id, cancellationToken);
            if (banner == null)
            {
                _logger.LogWarning("Banner bulunamadi. BannerId: {BannerId}", id);
                throw new NotFoundException("Banner", id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            banner.UpdateTitle(dto.Title);
            banner.UpdateDescription(dto.Description);
            banner.UpdateImageUrl(dto.ImageUrl);
            banner.UpdateLinkUrl(dto.LinkUrl);
            banner.UpdatePosition(dto.Position);
            banner.UpdateSortOrder(dto.SortOrder);
            banner.UpdateDateRange(dto.StartDate, dto.EndDate);
            banner.UpdateCategory(dto.CategoryId);
            banner.UpdateProduct(dto.ProductId);
            
            if (dto.IsActive)
                banner.Activate();
            else
                banner.Deactivate();

            await _bannerRepository.UpdateAsync(banner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Banner guncellendi. BannerId: {BannerId}, Title: {Title}", banner.Id, banner.Title);

            return _mapper.Map<BannerDto>(banner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner guncelleme hatasi. BannerId: {BannerId}, Title: {Title}", id, dto.Title);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var banner = await _bannerRepository.GetByIdAsync(id, cancellationToken);
        if (banner == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        banner.MarkAsDeleted();
        await _bannerRepository.UpdateAsync(banner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

