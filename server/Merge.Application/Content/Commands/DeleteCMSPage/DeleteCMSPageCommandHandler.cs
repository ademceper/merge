using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.CMSPage>;

namespace Merge.Application.Content.Commands.DeleteCMSPage;

public class DeleteCMSPageCommandHandler(
    IRepository cmsPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteCMSPageCommandHandler> logger) : IRequestHandler<DeleteCMSPageCommand, bool>
{
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";

    public async Task<bool> Handle(DeleteCMSPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting CMS page. PageId: {PageId}", request.Id);

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
                logger.LogWarning("Unauthorized attempt to delete CMS page {PageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, page.AuthorId.Value);
                throw new BusinessException("Bu CMS sayfasını silme yetkiniz bulunmamaktadır.");
            }

            var slug = page.Slug;
            var wasHomePage = page.IsHomePage;

            page.MarkAsDeleted();

            await cmsPageRepository.UpdateAsync(page, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("CMS page deleted successfully. PageId: {PageId}", request.Id);

            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{slug}", cancellationToken);
            if (wasHomePage)
            {
                await cache.RemoveAsync(CACHE_KEY_HOME_PAGE, cancellationToken);
            }
            await cache.RemoveAsync(CACHE_KEY_MENU_PAGES, cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting CMS page. PageId: {PageId}",
                request.Id);
            throw new BusinessException("CMS sayfası silme çakışması. Başka bir kullanıcı aynı sayfayı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting CMS page. PageId: {PageId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

