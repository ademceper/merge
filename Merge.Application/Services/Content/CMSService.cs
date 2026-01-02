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
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

namespace Merge.Application.Services.Content;

public class CMSService : ICMSService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CMSService> _logger;

    public CMSService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CMSService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CMSPageDto> CreatePageAsync(Guid? authorId, CreateCMSPageDto dto)
    {
        var slug = GenerateSlug(dto.Title);
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        if (await _context.Set<CMSPage>().AnyAsync(p => p.Slug == slug))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        // If setting as home page, unset other home pages
        if (dto.IsHomePage)
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var existingHomePages = await _context.Set<CMSPage>()
                .Where(p => p.IsHomePage)
                .ToListAsync();

            foreach (var existingPage in existingHomePages)
            {
                existingPage.IsHomePage = false;
            }
        }

        var page = new CMSPage
        {
            Title = dto.Title,
            Slug = slug,
            Content = dto.Content,
            Excerpt = dto.Excerpt,
            PageType = dto.PageType,
            Status = Enum.TryParse<ContentStatus>(dto.Status, true, out var statusEnum) ? statusEnum : ContentStatus.Draft,
            AuthorId = authorId,
            Template = dto.Template,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            MetaKeywords = dto.MetaKeywords,
            IsHomePage = dto.IsHomePage,
            DisplayOrder = dto.DisplayOrder,
            ShowInMenu = dto.ShowInMenu,
            MenuTitle = dto.MenuTitle,
            ParentPageId = dto.ParentPageId,
            PublishedAt = (Enum.TryParse<ContentStatus>(dto.Status, true, out var status) && status == ContentStatus.Published) ? DateTime.UtcNow : null
        };

        await _context.Set<CMSPage>().AddAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CMSPageDto>(page);
    }

    public async Task<CMSPageDto?> GetPageByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Id == id);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    public async Task<CMSPageDto?> GetPageBySlugAsync(string slug)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    public async Task<CMSPageDto?> GetHomePageAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.IsHomePage && p.Status == ContentStatus.Published);

        return page != null ? _mapper.Map<CMSPageDto>(page) : null;
    }

    // ✅ BOLUM 3.4: Pagination eklendi (ZORUNLU)
    public async Task<PagedResult<CMSPageDto>> GetAllPagesAsync(string? status = null, bool? showInMenu = null, int page = 1, int pageSize = 20)
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

        var totalCount = await query.CountAsync();

        var pages = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = pages.Select(p => _mapper.Map<CMSPageDto>(p)).ToList();

        return new PagedResult<CMSPageDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<CMSPageDto>> GetMenuPagesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var pages = await _context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .Where(p => p.ShowInMenu && p.Status == ContentStatus.Published && p.ParentPageId == null)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .ToListAsync();

        var result = new List<CMSPageDto>();
        foreach (var page in pages)
        {
            result.Add(_mapper.Map<CMSPageDto>(page));
        }
        return result;
    }

    public async Task<bool> UpdatePageAsync(Guid id, CreateCMSPageDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        if (!string.IsNullOrEmpty(dto.Title))
        {
            page.Title = dto.Title;
            page.Slug = GenerateSlug(dto.Title);
        }
        if (!string.IsNullOrEmpty(dto.Content))
            page.Content = dto.Content;
        if (dto.Excerpt != null)
            page.Excerpt = dto.Excerpt;
        if (!string.IsNullOrEmpty(dto.PageType))
            page.PageType = dto.PageType;
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(dto.Status, true, out var newStatus))
            {
                page.Status = newStatus;
                if (newStatus == ContentStatus.Published && !page.PublishedAt.HasValue)
                {
                    page.PublishedAt = DateTime.UtcNow;
                }
            }
        }
        if (dto.Template != null)
            page.Template = dto.Template;
        if (dto.MetaTitle != null)
            page.MetaTitle = dto.MetaTitle;
        if (dto.MetaDescription != null)
            page.MetaDescription = dto.MetaDescription;
        if (dto.MetaKeywords != null)
            page.MetaKeywords = dto.MetaKeywords;
        if (dto.IsHomePage)
        {
            // Unset other home pages
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var existingHomePages = await _context.Set<CMSPage>()
                .Where(p => p.IsHomePage && p.Id != id)
                .ToListAsync();

            foreach (var p in existingHomePages)
            {
                p.IsHomePage = false;
            }
            page.IsHomePage = true;
        }
        page.DisplayOrder = dto.DisplayOrder;
        page.ShowInMenu = dto.ShowInMenu;
        if (dto.MenuTitle != null)
            page.MenuTitle = dto.MenuTitle;
        if (dto.ParentPageId.HasValue)
            page.ParentPageId = dto.ParentPageId;

        page.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        page.IsDeleted = true;
        page.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishPageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        page.Status = ContentStatus.Published;
        page.PublishedAt = DateTime.UtcNow;
        page.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetHomePageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<CMSPage>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        // Unset other home pages
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var existingHomePages = await _context.Set<CMSPage>()
            .Where(p => p.IsHomePage && p.Id != id)
            .ToListAsync();

        foreach (var existingPage in existingHomePages)
        {
            existingPage.IsHomePage = false;
        }

        page.IsHomePage = true;
        page.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

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

