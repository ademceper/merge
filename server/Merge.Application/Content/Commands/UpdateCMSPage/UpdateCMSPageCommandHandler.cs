using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.UpdateCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateCMSPageCommandHandler(
    Merge.Application.Interfaces.IRepository<CMSPage> cmsPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdateCMSPageCommandHandler> logger) : IRequestHandler<UpdateCMSPageCommand, bool>
{
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";

    public async Task<bool> Handle(UpdateCMSPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating CMS page. PageId: {PageId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var page = await cmsPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (page == null)
            {
                logger.LogWarning("CMS page not found. PageId: {PageId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Manager sadece kendi sayfalarını güncelleyebilmeli (Admin hariç)
            // PerformedBy null ise (Admin), tüm sayfaları güncelleyebilir
            // PerformedBy null değilse (Manager), sadece kendi sayfalarını güncelleyebilir
            if (request.PerformedBy.HasValue && page.AuthorId.HasValue && page.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to update CMS page {PageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, page.AuthorId.Value);
                throw new BusinessException("Bu CMS sayfasını güncelleme yetkiniz bulunmamaktadır.");
            }

            // Store old values for cache invalidation
            var oldSlug = page.Slug;
            var wasHomePage = page.IsHomePage;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (!string.IsNullOrEmpty(request.Title))
            {
                page.UpdateTitle(request.Title);
            }
            if (!string.IsNullOrEmpty(request.Content))
                page.UpdateContent(request.Content);
            if (request.Excerpt != null)
                page.UpdateExcerpt(request.Excerpt);
            if (!string.IsNullOrEmpty(request.PageType))
                page.UpdatePageType(request.PageType);
            if (!string.IsNullOrEmpty(request.Status))
            {
                // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
                if (Enum.TryParse<ContentStatus>(request.Status, true, out var newStatus))
                {
                    page.UpdateStatus(newStatus);
                }
            }
            if (request.Template != null)
                page.UpdateTemplate(request.Template);
            if (request.MetaTitle != null || request.MetaDescription != null || request.MetaKeywords != null)
                page.UpdateMetaInformation(request.MetaTitle, request.MetaDescription, request.MetaKeywords);
            if (request.IsHomePage.HasValue && request.IsHomePage.Value)
            {
                // Unset other home pages
                var existingHomePages = await context.Set<CMSPage>()
                    .Where(p => p.IsHomePage && p.Id != request.Id)
                    .ToListAsync(cancellationToken);

                foreach (var existingPage in existingHomePages)
                {
                    existingPage.UnsetAsHomePage();
                }
                page.SetAsHomePage();
            }
            if (request.DisplayOrder.HasValue)
                page.UpdateDisplayOrder(request.DisplayOrder.Value);
            if (request.ShowInMenu.HasValue)
                page.UpdateShowInMenu(request.ShowInMenu.Value);
            if (request.MenuTitle != null)
                page.UpdateMenuTitle(request.MenuTitle);
            if (request.ParentPageId.HasValue)
                page.UpdateParentPage(request.ParentPageId);

            await cmsPageRepository.UpdateAsync(page, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("CMS page updated successfully. PageId: {PageId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all CMS page-related cache
            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}", cancellationToken);
            if (oldSlug != page.Slug)
            {
                await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{oldSlug}", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{page.Slug}", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{page.Slug}", cancellationToken);
            }
            if (wasHomePage || request.IsHomePage == true)
            {
                await cache.RemoveAsync(CACHE_KEY_HOME_PAGE, cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_MENU_PAGES, cancellationToken);
            // Note: Paginated cache'ler (cms_pages_all_paged_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating CMS page. PageId: {PageId}",
                request.Id);
            throw new BusinessException("CMS sayfası güncelleme çakışması. Başka bir kullanıcı aynı sayfayı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            logger.LogError(ex, "Error updating CMS page. PageId: {PageId}", request.Id);
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

