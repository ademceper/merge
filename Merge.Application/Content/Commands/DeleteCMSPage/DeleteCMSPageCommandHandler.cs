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

namespace Merge.Application.Content.Commands.DeleteCMSPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteCMSPageCommandHandler : IRequestHandler<DeleteCMSPageCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<CMSPage> _cmsPageRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteCMSPageCommandHandler> _logger;
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";

    public DeleteCMSPageCommandHandler(
        Merge.Application.Interfaces.IRepository<CMSPage> cmsPageRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteCMSPageCommandHandler> logger)
    {
        _cmsPageRepository = cmsPageRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCMSPageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting CMS page. PageId: {PageId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var page = await _cmsPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (page == null)
            {
                _logger.LogWarning("CMS page not found. PageId: {PageId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Manager sadece kendi sayfalarını silebilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && page.AuthorId.HasValue && page.AuthorId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to delete CMS page {PageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, page.AuthorId.Value);
                throw new BusinessException("Bu CMS sayfasını silme yetkiniz bulunmamaktadır.");
            }

            // Store values for cache invalidation
            var slug = page.Slug;
            var wasHomePage = page.IsHomePage;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            page.MarkAsDeleted();

            await _cmsPageRepository.UpdateAsync(page, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("CMS page deleted successfully. PageId: {PageId}", request.Id);

            // ✅ BOLUM 10.2: Cache invalidation - Remove all CMS page-related cache
            await _cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_CMS_PAGE_BY_SLUG}{slug}", cancellationToken);
            if (wasHomePage)
            {
                await _cache.RemoveAsync(CACHE_KEY_HOME_PAGE, cancellationToken);
            }
            await _cache.RemoveAsync(CACHE_KEY_MENU_PAGES, cancellationToken);
            // Note: Paginated cache'ler (cms_pages_all_paged_*) pattern-based invalidation gerektirir.
            // Şimdilik cache expiration'a güveniyoruz (15 dakika TTL)

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting CMS page. PageId: {PageId}",
                request.Id);
            throw new BusinessException("CMS sayfası silme çakışması. Başka bir kullanıcı aynı sayfayı güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting CMS page. PageId: {PageId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

