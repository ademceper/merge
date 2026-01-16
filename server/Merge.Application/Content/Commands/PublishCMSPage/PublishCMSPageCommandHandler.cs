using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.CMSPage>;

namespace Merge.Application.Content.Commands.PublishCMSPage;

public class PublishCMSPageCommandHandler(
    IRepository cmsPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<PublishCMSPageCommandHandler> logger) : IRequestHandler<PublishCMSPageCommand, bool>
{
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";

    public async Task<bool> Handle(PublishCMSPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Publishing CMS page. PageId: {PageId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var page = await cmsPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (page == null)
            {
                logger.LogWarning("CMS page not found. PageId: {PageId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && page.AuthorId.HasValue && page.AuthorId.Value != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to publish CMS page {PageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, page.AuthorId.Value);
                throw new BusinessException("Bu CMS sayfasını yayınlama yetkiniz bulunmamaktadır.");
            }

            page.Publish();

            await cmsPageRepository.UpdateAsync(page, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("CMS page published successfully. PageId: {PageId}", request.Id);

            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{page.Slug}", cancellationToken);
            if (page.IsHomePage)
            {
                await cache.RemoveAsync(CACHE_KEY_HOME_PAGE, cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_MENU_PAGES, cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while publishing CMS page. PageId: {PageId}",
                request.Id);
            throw new BusinessException("CMS sayfası yayınlama çakışması. Başka bir kullanıcı aynı sayfayı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing CMS page. PageId: {PageId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

