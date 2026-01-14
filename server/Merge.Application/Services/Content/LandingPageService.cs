using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Content;

public class LandingPageService : ILandingPageService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LandingPageService> _logger;

    public LandingPageService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LandingPageService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use CreateLandingPageCommand via MediatR instead")]
    public async Task<LandingPageDto> CreateLandingPageAsync(Guid? authorId, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateLandingPageDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        _logger.LogInformation("Landing page olusturuluyor. Name: {Name}, AuthorId: {AuthorId}", dto.Name, authorId);

        try
        {
            var slug = GenerateSlug(dto.Name);
            // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
            if (await _context.Set<LandingPage>().AnyAsync(lp => lp.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var status = Enum.TryParse<ContentStatus>(dto.Status, true, out var statusEnum) ? statusEnum : ContentStatus.Draft;
            var landingPage = LandingPage.Create(
                name: dto.Name,
                title: dto.Title,
                content: dto.Content,
                authorId: authorId,
                template: dto.Template,
                status: status,
                startDate: dto.StartDate,
                endDate: dto.EndDate,
                metaTitle: dto.MetaTitle,
                metaDescription: dto.MetaDescription,
                ogImageUrl: dto.OgImageUrl,
                enableABTesting: dto.EnableABTesting,
                variantOfId: dto.VariantOfId,
                trafficSplit: dto.TrafficSplit,
                slug: slug);

            await _context.Set<LandingPage>().AddAsync(landingPage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Landing page olusturuldu. LandingPageId: {LandingPageId}, Name: {Name}, Slug: {Slug}", 
                landingPage.Id, landingPage.Name, landingPage.Slug);

            return _mapper.Map<LandingPageDto>(landingPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Landing page olusturma hatasi. Name: {Name}, AuthorId: {AuthorId}", dto.Name, authorId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<LandingPageDto?> GetLandingPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .AsNoTracking()
            .Include(lp => lp.Author)
            .Include(lp => lp.VariantOf)
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        return landingPage != null ? _mapper.Map<LandingPageDto>(landingPage) : null;
    }

    public async Task<LandingPageDto?> GetLandingPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        // ⚠️ NOTE: AsNoTracking kullanılmıyor çünkü view count increment edilecek (tracking gerekli)
        var landingPage = await _context.Set<LandingPage>()
            .Include(lp => lp.Author)
            .Include(lp => lp.VariantOf)
            .FirstOrDefaultAsync(lp => lp.Slug == slug && lp.Status == ContentStatus.Published && lp.IsActive, cancellationToken);

        if (landingPage != null)
        {
            // Check if A/B testing is enabled and select variant
            if (landingPage.EnableABTesting && landingPage.Variants != null && landingPage.Variants.Any())
            {
                // ✅ PERFORMANCE: Removed manual !v.IsDeleted (Global Query Filter)
                var variants = landingPage.Variants.Where(v => v.IsActive).ToList();
                if (variants.Any())
                {
                    // Simple random selection based on traffic split (can be improved with proper A/B testing logic)
                    // ✅ THREAD SAFETY: Random.Shared kullan (new Random() thread-safe değil)
                    var random = Random.Shared;
                    var selectedVariant = variants.OrderBy(v => random.Next()).FirstOrDefault();
                    if (selectedVariant != null)
                    {
                        landingPage = selectedVariant;
                    }
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            landingPage.IncrementViewCount();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return landingPage != null ? _mapper.Map<LandingPageDto>(landingPage) : null;
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    public async Task<PagedResult<LandingPageDto>> GetAllLandingPagesAsync(string? status = null, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<LandingPage>()
            .Include(lp => lp.Author)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !lp.IsDeleted (Global Query Filter)
            .AsNoTracking()
            .Where(lp => lp.VariantOfId == null); // Only show original pages, not variants

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(status, true, out var statusEnum))
            {
                query = query.Where(lp => lp.Status == statusEnum);
            }
        }

        if (isActive.HasValue)
        {
            query = query.Where(lp => lp.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var landingPages = await query
            .OrderByDescending(lp => lp.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = landingPages.Select(lp => _mapper.Map<LandingPageDto>(lp)).ToList();

        return new PagedResult<LandingPageDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    [Obsolete("Use UpdateLandingPageCommand via MediatR instead")]
    public async Task<bool> UpdateLandingPageAsync(Guid id, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateLandingPageDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        if (landingPage == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(dto.Name))
        {
            landingPage.UpdateName(dto.Name);
        }
        if (!string.IsNullOrEmpty(dto.Title))
            landingPage.UpdateTitle(dto.Title);
        if (!string.IsNullOrEmpty(dto.Content))
            landingPage.UpdateContent(dto.Content);
        if (dto.Template != null)
            landingPage.UpdateTemplate(dto.Template);
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(dto.Status, true, out var newStatus))
            {
                landingPage.UpdateStatus(newStatus);
            }
        }
        if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            landingPage.UpdateSchedule(dto.StartDate, dto.EndDate);
        if (dto.MetaTitle != null || dto.MetaDescription != null || dto.OgImageUrl != null)
            landingPage.UpdateMetaInformation(dto.MetaTitle, dto.MetaDescription, dto.OgImageUrl);
        landingPage.UpdateABTestingSettings(dto.EnableABTesting, dto.TrafficSplit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteLandingPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        if (landingPage == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        landingPage.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PublishLandingPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        if (landingPage == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        landingPage.Publish();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> TrackConversionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        if (landingPage == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        landingPage.TrackConversion();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    [Obsolete("Use CreateLandingPageCommand via MediatR instead")]
    public async Task<LandingPageDto> CreateVariantAsync(Guid originalId, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateLandingPageDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var original = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == originalId, cancellationToken);

        if (original == null)
        {
            throw new NotFoundException("Orijinal landing page", dto.VariantOfId ?? Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var variantStatus = Enum.TryParse<ContentStatus>(dto.Status, true, out var status) ? status : ContentStatus.Draft;
        var variant = original.CreateVariant(
            name: $"{original.Name} - Variant",
            title: dto.Title ?? original.Title,
            content: dto.Content ?? original.Content,
            template: dto.Template ?? original.Template,
            status: variantStatus,
            startDate: dto.StartDate ?? original.StartDate,
            endDate: dto.EndDate ?? original.EndDate,
            metaTitle: dto.MetaTitle ?? original.MetaTitle,
            metaDescription: dto.MetaDescription ?? original.MetaDescription,
            ogImageUrl: dto.OgImageUrl ?? original.OgImageUrl,
            trafficSplit: dto.TrafficSplit);

        await _context.Set<LandingPage>().AddAsync(variant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LandingPageDto>(variant);
    }

    public async Task<LandingPageAnalyticsDto> GetLandingPageAnalyticsAsync(Guid id, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .Include(lp => lp.Variants)
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

        if (landingPage == null)
        {
            throw new NotFoundException("Landing page", id);
        }

        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var variants = landingPage.Variants != null && landingPage.Variants.Any()
            ? landingPage.Variants.Select(v => _mapper.Map<LandingPageVariantDto>(v)).ToList()
            : new List<LandingPageVariantDto>();

        return new LandingPageAnalyticsDto(
            landingPage.Id,
            landingPage.Name,
            landingPage.ViewCount,
            landingPage.ConversionCount,
            landingPage.ConversionRate,
            new Dictionary<string, int>(), // ViewsByDate - Gerçek implementasyonda hesaplanmalı
            new Dictionary<string, int>(), // ConversionsByDate - Gerçek implementasyonda hesaplanmalı
            variants
        );
    }

    // ✅ BOLUM 1.1: Helper method - Slug generation artık entity içinde (GenerateSlug private method)
    // Bu method sadece uniqueness kontrolü için kullanılıyor
    private string GenerateSlug(string name)
    {
        // Slug generation artık LandingPage entity içinde yapılıyor
        // Bu method sadece service layer'da uniqueness kontrolü için kullanılıyor
        var slug = name.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

}

