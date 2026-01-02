using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Content;


namespace Merge.Application.Services.Content;

public class PageBuilderService : IPageBuilderService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PageBuilderService> _logger;
    private readonly IMapper _mapper;

    public PageBuilderService(ApplicationDbContext context, IUnitOfWork unitOfWork, ILogger<PageBuilderService> logger, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PageBuilderDto> CreatePageAsync(CreatePageBuilderDto dto)
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

        await _context.PageBuilders.AddAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<PageBuilderDto>(page);
    }

    public async Task<PageBuilderDto?> GetPageAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.PageBuilders
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        return page != null ? _mapper.Map<PageBuilderDto>(page) : null;
    }

    public async Task<PageBuilderDto?> GetPageBySlugAsync(string slug)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.PageBuilders
            .AsNoTracking()
            .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ContentStatus.Published);

        return page != null ? _mapper.Map<PageBuilderDto>(page) : null;
    }

    public async Task<IEnumerable<PageBuilderDto>> GetAllPagesAsync(string? status = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var query = _context.PageBuilders
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

        var pages = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<PageBuilderDto>();
        foreach (var p in pages)
        {
            result.Add(_mapper.Map<PageBuilderDto>(p));
        }
        return result;
    }

    public async Task<bool> UpdatePageAsync(Guid id, UpdatePageBuilderDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.PageBuilders
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        if (!string.IsNullOrEmpty(dto.Name)) page.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Slug)) page.Slug = dto.Slug;
        if (!string.IsNullOrEmpty(dto.Title)) page.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Content)) page.Content = dto.Content;
        if (dto.Template != null) page.Template = dto.Template;
        if (dto.MetaTitle != null) page.MetaTitle = dto.MetaTitle;
        if (dto.MetaDescription != null) page.MetaDescription = dto.MetaDescription;
        if (dto.OgImageUrl != null) page.OgImageUrl = dto.OgImageUrl;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.PageBuilders
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
        var page = await _context.PageBuilders
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        page.Status = ContentStatus.Published;
        page.PublishedAt = DateTime.UtcNow;
        page.IsActive = true;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishPageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var page = await _context.PageBuilders
            .FirstOrDefaultAsync(p => p.Id == id);

        if (page == null) return false;

        page.Status = ContentStatus.Draft;
        page.IsActive = false;
        page.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

}

