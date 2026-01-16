using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.CMSPage>;

namespace Merge.Application.Content.Commands.CreateCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateCMSPageCommandHandler(
    IRepository cmsPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateCMSPageCommandHandler> logger) : IRequestHandler<CreateCMSPageCommand, CMSPageDto>
{
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";

    public async Task<CMSPageDto> Handle(CreateCMSPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating CMS page. AuthorId: {AuthorId}, Title: {Title}",
            request.AuthorId, request.Title);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // If setting as home page, unset other home pages
            if (request.IsHomePage)
            {
                var existingHomePages = await context.Set<CMSPage>()
                    .Where(p => p.IsHomePage)
                    .ToListAsync(cancellationToken);

                foreach (var existingPage in existingHomePages)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                    existingPage.UnsetAsHomePage();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var statusEnum = Enum.TryParse<ContentStatus>(request.Status, true, out var status) ? status : ContentStatus.Draft;
            var page = CMSPage.Create(
                request.Title,
                request.Content,
                request.AuthorId,
                request.Excerpt,
                request.PageType,
                statusEnum,
                request.Template,
                request.MetaTitle,
                request.MetaDescription,
                request.MetaKeywords,
                request.IsHomePage,
                request.DisplayOrder,
                request.ShowInMenu,
                request.MenuTitle,
                request.ParentPageId);

            page = await cmsPageRepository.AddAsync(page, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedPage = await context.Set<CMSPage>()
                .AsNoTracking()
                .Include(p => p.Author)
                .Include(p => p.ParentPage)
                .FirstOrDefaultAsync(p => p.Id == page.Id, cancellationToken);

            if (reloadedPage == null)
            {
                logger.LogWarning("CMS page not found after creation. PageId: {PageId}", page.Id);
                throw new NotFoundException("CMS Sayfası", page.Id);
            }

            logger.LogInformation("CMS page created successfully. PageId: {PageId}, Slug: {Slug}",
                page.Id, page.Slug);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all CMS page-related cache
            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_ID}{page.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{reloadedPage.Slug}", cancellationToken);
            if (request.IsHomePage)
            {
                await cache.RemoveAsync(CACHE_KEY_HOME_PAGE, cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_MENU_PAGES, cancellationToken);
            // Note: Paginated cache'ler (cms_pages_all_paged_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)

            return mapper.Map<CMSPageDto>(reloadedPage);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating CMS page. Title: {Title}",
                request.Title);
            throw new BusinessException("CMS sayfası oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error creating CMS page. Title: {Title}", request.Title);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateSlug(string title)
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

