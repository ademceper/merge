using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Content;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;


namespace Merge.Application.Services.Content;

public class LandingPageService : ILandingPageService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LandingPageService> _logger;

    public LandingPageService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LandingPageService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LandingPageDto> CreateLandingPageAsync(Guid? authorId, CreateLandingPageDto dto)
    {
        var slug = GenerateSlug(dto.Name);
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        if (await _context.Set<LandingPage>().AnyAsync(lp => lp.Slug == slug))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var landingPage = new LandingPage
        {
            Name = dto.Name,
            Slug = slug,
            Title = dto.Title,
            Content = dto.Content,
            Template = dto.Template,
            Status = Enum.TryParse<ContentStatus>(dto.Status, true, out var statusEnum) ? statusEnum : ContentStatus.Draft,
            AuthorId = authorId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            OgImageUrl = dto.OgImageUrl,
            EnableABTesting = dto.EnableABTesting,
            VariantOfId = dto.VariantOfId,
            TrafficSplit = dto.TrafficSplit,
            PublishedAt = (Enum.TryParse<ContentStatus>(dto.Status, true, out var status) && status == ContentStatus.Published) ? DateTime.UtcNow : null
        };

        await _context.Set<LandingPage>().AddAsync(landingPage);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LandingPageDto>(landingPage);
    }

    public async Task<LandingPageDto?> GetLandingPageByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .AsNoTracking()
            .Include(lp => lp.Author)
            .Include(lp => lp.VariantOf)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        return landingPage != null ? _mapper.Map<LandingPageDto>(landingPage) : null;
    }

    public async Task<LandingPageDto?> GetLandingPageBySlugAsync(string slug)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        // ⚠️ NOTE: AsNoTracking kullanılmıyor çünkü view count increment edilecek (tracking gerekli)
        var landingPage = await _context.Set<LandingPage>()
            .Include(lp => lp.Author)
            .Include(lp => lp.VariantOf)
            .FirstOrDefaultAsync(lp => lp.Slug == slug && lp.Status == ContentStatus.Published && lp.IsActive);

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

            // Increment view count
            landingPage.ViewCount++;
            await _unitOfWork.SaveChangesAsync();
        }

        return landingPage != null ? _mapper.Map<LandingPageDto>(landingPage) : null;
    }

    public async Task<IEnumerable<LandingPageDto>> GetAllLandingPagesAsync(string? status = null, bool? isActive = null)
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

        var landingPages = await query
            .OrderByDescending(lp => lp.CreatedAt)
            .ToListAsync();

        var result = new List<LandingPageDto>();
        foreach (var landingPage in landingPages)
        {
            result.Add(_mapper.Map<LandingPageDto>(landingPage));
        }
        return result;
    }

    public async Task<bool> UpdateLandingPageAsync(Guid id, CreateLandingPageDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (landingPage == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
        {
            landingPage.Name = dto.Name;
            landingPage.Slug = GenerateSlug(dto.Name);
        }
        if (!string.IsNullOrEmpty(dto.Title))
            landingPage.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Content))
            landingPage.Content = dto.Content;
        if (dto.Template != null)
            landingPage.Template = dto.Template;
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(dto.Status, true, out var newStatus))
            {
                landingPage.Status = newStatus;
                if (newStatus == ContentStatus.Published && !landingPage.PublishedAt.HasValue)
                {
                    landingPage.PublishedAt = DateTime.UtcNow;
                }
            }
        }
        if (dto.StartDate.HasValue)
            landingPage.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue)
            landingPage.EndDate = dto.EndDate;
        if (dto.MetaTitle != null)
            landingPage.MetaTitle = dto.MetaTitle;
        if (dto.MetaDescription != null)
            landingPage.MetaDescription = dto.MetaDescription;
        if (dto.OgImageUrl != null)
            landingPage.OgImageUrl = dto.OgImageUrl;
        landingPage.EnableABTesting = dto.EnableABTesting;
        landingPage.TrafficSplit = dto.TrafficSplit;

        landingPage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteLandingPageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (landingPage == null) return false;

        landingPage.IsDeleted = true;
        landingPage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishLandingPageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (landingPage == null) return false;

        landingPage.Status = ContentStatus.Published;
        landingPage.PublishedAt = DateTime.UtcNow;
        landingPage.IsActive = true;
        landingPage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TrackConversionAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (landingPage == null) return false;

        landingPage.ConversionCount++;
        if (landingPage.ViewCount > 0)
        {
            landingPage.ConversionRate = (decimal)landingPage.ConversionCount / landingPage.ViewCount * 100;
        }
        landingPage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<LandingPageDto> CreateVariantAsync(Guid originalId, CreateLandingPageDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var original = await _context.Set<LandingPage>()
            .FirstOrDefaultAsync(lp => lp.Id == originalId);

        if (original == null)
        {
            throw new NotFoundException("Orijinal landing page", dto.VariantOfId ?? Guid.Empty);
        }

        var variant = new LandingPage
        {
            Name = $"{original.Name} - Variant",
            Slug = $"{original.Slug}-variant-{DateTime.UtcNow.Ticks}",
            Title = dto.Title ?? original.Title,
            Content = dto.Content ?? original.Content,
            Template = dto.Template ?? original.Template,
            Status = Enum.TryParse<ContentStatus>(dto.Status, true, out var variantStatus) ? variantStatus : ContentStatus.Draft,
            AuthorId = dto.VariantOfId.HasValue ? original.AuthorId : null,
            StartDate = dto.StartDate ?? original.StartDate,
            EndDate = dto.EndDate ?? original.EndDate,
            IsActive = true,
            MetaTitle = dto.MetaTitle ?? original.MetaTitle,
            MetaDescription = dto.MetaDescription ?? original.MetaDescription,
            OgImageUrl = dto.OgImageUrl ?? original.OgImageUrl,
            EnableABTesting = true,
            VariantOfId = originalId,
            TrafficSplit = dto.TrafficSplit
        };

        await _context.Set<LandingPage>().AddAsync(variant);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LandingPageDto>(variant);
    }

    public async Task<LandingPageAnalyticsDto> GetLandingPageAnalyticsAsync(Guid id, DateTime? startDate = null, DateTime? endDate = null)
    {
        // ✅ PERFORMANCE: Removed manual !lp.IsDeleted (Global Query Filter)
        var landingPage = await _context.Set<LandingPage>()
            .Include(lp => lp.Variants)
            .FirstOrDefaultAsync(lp => lp.Id == id);

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

        return new LandingPageAnalyticsDto
        {
            LandingPageId = landingPage.Id,
            LandingPageName = landingPage.Name,
            TotalViews = landingPage.ViewCount,
            TotalConversions = landingPage.ConversionCount,
            ConversionRate = landingPage.ConversionRate,
            Variants = variants
        };
    }

    private string GenerateSlug(string name)
    {
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

