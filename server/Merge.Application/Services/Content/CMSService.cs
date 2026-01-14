using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Content;

public class CMSService : ICMSService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CMSService> _logger;

    public CMSService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CMSService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use CreateCMSPageCommand via MediatR instead")]
    public async Task<CMSPageDto> CreatePageAsync(Guid? authorId, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateCMSPageDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        _logger.LogInformation("CMS sayfasi olusturuluyor. AuthorId: {AuthorId}, Title: {Title}", authorId, dto.Title);

        try
        {
            var slug = GenerateSlug(dto.Title);
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            if (await _context.Set<CMSPage>().AnyAsync(p => p.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // If setting as home page, unset other home pages
            if (dto.IsHomePage)
            {
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
                var existingHomePages = await _context.Set<CMSPage>()
                    .Where(p => p.IsHomePage)
                    .ToListAsync(cancellationToken);

                foreach (var existingPage in existingHomePages)
                {
                    existingPage.UnsetAsHomePage();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var statusEnum = Enum.TryParse<ContentStatus>(dto.Status, true, out var status) ? status : ContentStatus.Draft;
            var page = CMSPage.Create(
                dto.Title,
                dto.Content,
                authorId,
                dto.Excerpt,
                dto.PageType,
                statusEnum,
                dto.Template,
                dto.MetaTitle,
                dto.MetaDescription,
                dto.MetaKeywords,
                dto.IsHomePage,
                dto.DisplayOrder,
                dto.ShowInMenu,
                dto.MenuTitle,
                dto.ParentPageId);

            await _context.Set<CMSPage>().AddAsync(page, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CMS sayfasi olusturuldu. PageId: {PageId}, Slug: {Slug}", page.Id, page.Slug);

            return _mapper.Map<CMSPageDto>(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CMS sayfasi olusturma hatasi. AuthorId: {AuthorId}, Title: {Title}", authorId, dto.Title);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<CMSPageDto?> GetPageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    public async Task<CMSPageDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published, cancellationToken);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    public async Task<CMSPageDto?> GetHomePageAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.IsHomePage && p.Status == ContentStatus.Published, cancellationToken);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    // ✅ BOLUM 3.4: Pagination eklendi (ZORUNLU)
    public async Task<PagedResult<CMSPageDto>> GetAllPagesAsync(string? status = null, bool? showInMenu = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        IQueryable<CMSPage> query = _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage);

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }
        }

        if (showInMenu.HasValue)
        {
            query = query.Where(p => p.ShowInMenu == showInMenu.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pages = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pages.Select(p => _mapper.Map<CMSPageDto>(p)).ToList();

        return new PagedResult<CMSPageDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<CMSPageDto>> GetMenuPagesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Menü sayfaları genelde sınırlı (10-20) ama güvenlik için limit ekle
        var pages = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .Where(p => p.ShowInMenu && p.Status == ContentStatus.Published && p.ParentPageId == null)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Take(100) // ✅ Güvenlik: Maksimum 100 menü sayfası
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<CMSPageDto>(pages.Count);
        foreach (var page in pages)
        {
            result.Add(_mapper.Map<CMSPageDto>(page));
        }
        return result;
    }

    [Obsolete("Use UpdateCMSPageCommand via MediatR instead")]
    public async Task<bool> UpdatePageAsync(Guid id, object dtoObj, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreateCMSPageDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }
        
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(dto.Title))
        {
            page.UpdateTitle(dto.Title);
        }
        if (!string.IsNullOrEmpty(dto.Content))
            page.UpdateContent(dto.Content);
        if (dto.Excerpt != null)
            page.UpdateExcerpt(dto.Excerpt);
        if (!string.IsNullOrEmpty(dto.PageType))
            page.UpdatePageType(dto.PageType);
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(dto.Status, true, out var newStatus))
            {
                page.UpdateStatus(newStatus);
            }
        }
        if (dto.Template != null)
            page.UpdateTemplate(dto.Template);
        if (dto.MetaTitle != null || dto.MetaDescription != null || dto.MetaKeywords != null)
            page.UpdateMetaInformation(dto.MetaTitle, dto.MetaDescription, dto.MetaKeywords);
        if (dto.IsHomePage)
        {
            // Unset other home pages
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var existingHomePages = await _context.Set<CMSPage>()
                .Where(p => p.IsHomePage && p.Id != id)
                .ToListAsync(cancellationToken);

            foreach (var p in existingHomePages)
            {
                p.UnsetAsHomePage();
            }
            page.SetAsHomePage();
        }
        page.UpdateDisplayOrder(dto.DisplayOrder);
        page.UpdateShowInMenu(dto.ShowInMenu);
        if (dto.MenuTitle != null)
            page.UpdateMenuTitle(dto.MenuTitle);
        if (dto.ParentPageId.HasValue)
            page.UpdateParentPage(dto.ParentPageId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeletePageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
        page.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PublishPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        page.Publish();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SetHomePageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        // Unset other home pages
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var existingHomePages = await _context.Set<CMSPage>()
            .Where(p => p.IsHomePage && p.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var existingPage in existingHomePages)
        {
            existingPage.UnsetAsHomePage();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        page.SetAsHomePage();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
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

