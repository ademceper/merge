using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;


namespace Merge.Application.Services.Content;

public class PageBuilderService : IPageBuilderService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PageBuilderService> _logger;
    private readonly IMapper _mapper;

    public PageBuilderService(IDbContext context, IUnitOfWork unitOfWork, ILogger<PageBuilderService> logger, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<PageBuilderDto> CreatePageAsync(CreatePageBuilderDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Page builder sayfasi olusturuluyor. Name: {Name}, AuthorId: {AuthorId}", dto.Name, dto.AuthorId);

        try
        {
            var page = new PageBuilder
            {
                Name = dto.Name,
                Slug = dto.Slug,
                Title = dto.Title,
                Content = dto.Content,
                Template = dto.Template,
                Status = ContentStatus.Draft,
                AuthorId = dto.AuthorId,
                PageType = dto.PageType,
                RelatedEntityId = dto.RelatedEntityId,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                OgImageUrl = dto.OgImageUrl
            };

            await _context.Set<PageBuilder>().AddAsync(page, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Page builder sayfasi olusturuldu. PageId: {PageId}, Name: {Name}", page.Id, page.Name);

            return _mapper.Map<PageBuilderDto>(page);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Page builder sayfasi olusturma hatasi. Name: {Name}, AuthorId: {AuthorId}", dto.Name, dto.AuthorId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<PageBuilderDto?> GetPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<PageBuilder>()
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return page != null ? _mapper.Map<PageBuilderDto>(page) : null;
    }

    public async Task<PageBuilderDto?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<PageBuilder>()
            .AsNoTracking()
            .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published, cancellationToken);

        return page != null ? _mapper.Map<PageBuilderDto>(page) : null;
    }

    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    public async Task<PagedResult<PageBuilderDto>> GetAllPagesAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.Set<PageBuilder>()
            .AsNoTracking()
            .Include(p => p.Author)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<ContentStatus>(status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pages = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = pages.Select(p => _mapper.Map<PageBuilderDto>(p)).ToList();

        return new PagedResult<PageBuilderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<bool> UpdatePageAsync(Guid id, UpdatePageBuilderDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Page builder sayfasi guncelleniyor. PageId: {PageId}, Name: {Name}", id, dto.Name);

        try
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var page = await _context.Set<PageBuilder>()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (page == null)
            {
                _logger.LogWarning("Page builder sayfasi bulunamadi. PageId: {PageId}", id);
                return false;
            }

            if (!string.IsNullOrEmpty(dto.Name)) page.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Slug)) page.Slug = dto.Slug;
            if (!string.IsNullOrEmpty(dto.Title)) page.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Content)) page.Content = dto.Content;
            if (dto.Template != null) page.Template = dto.Template;
            if (dto.MetaTitle != null) page.MetaTitle = dto.MetaTitle;
            if (dto.MetaDescription != null) page.MetaDescription = dto.MetaDescription;
            if (dto.OgImageUrl != null) page.OgImageUrl = dto.OgImageUrl;
            page.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Page builder sayfasi guncellendi. PageId: {PageId}, Name: {Name}", page.Id, page.Name);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Page builder sayfasi guncelleme hatasi. PageId: {PageId}, Name: {Name}", id, dto.Name);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    public async Task<bool> DeletePageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<PageBuilder>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        page.IsDeleted = true;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PublishPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<PageBuilder>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        page.Status = ContentStatus.Published;
        page.PublishedAt = DateTime.UtcNow;
        page.IsActive = true;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnpublishPageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.Set<PageBuilder>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null) return false;

        page.Status = ContentStatus.Draft;
        page.IsActive = false;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

}

