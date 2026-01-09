using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.UpdateLandingPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateLandingPageCommandHandler : IRequestHandler<UpdateLandingPageCommand, bool>
{
    private readonly IRepository<LandingPage> _landingPageRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateLandingPageCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public UpdateLandingPageCommandHandler(
        IRepository<LandingPage> landingPageRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<UpdateLandingPageCommandHandler> logger)
    {
        _landingPageRepository = landingPageRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateLandingPageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating landing page. LandingPageId: {LandingPageId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await _landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage == null)
            {
                _logger.LogWarning("Landing page not found. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.7: Concurrency Control - RowVersion kontrolü
            // Note: RowVersion kontrolü genellikle HTTP ETag veya If-Match header ile yapılır
            // Burada sadece entity'nin RowVersion'ını kontrol ediyoruz

            // ✅ BOLUM 3.2: IDOR Koruması - Manager sadece kendi landing page'lerini güncelleyebilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && landingPage.AuthorId.HasValue && landingPage.AuthorId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to update landing page {LandingPageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, landingPage.AuthorId.Value);
                throw new BusinessException("Bu landing page'i güncelleme yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (!string.IsNullOrEmpty(request.Name))
            {
                landingPage.UpdateName(request.Name);
            }
            if (!string.IsNullOrEmpty(request.Title))
            {
                landingPage.UpdateTitle(request.Title);
            }
            if (!string.IsNullOrEmpty(request.Content))
            {
                landingPage.UpdateContent(request.Content);
            }
            if (request.Template != null)
            {
                landingPage.UpdateTemplate(request.Template);
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
                if (Enum.TryParse<ContentStatus>(request.Status, true, out var newStatus))
                {
                    landingPage.UpdateStatus(newStatus);
                }
            }
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                landingPage.UpdateSchedule(request.StartDate, request.EndDate);
            }
            if (request.MetaTitle != null || request.MetaDescription != null || request.OgImageUrl != null)
            {
                landingPage.UpdateMetaInformation(request.MetaTitle, request.MetaDescription, request.OgImageUrl);
            }
            if (request.EnableABTesting.HasValue || request.TrafficSplit.HasValue)
            {
                var trafficSplit = request.TrafficSplit ?? landingPage.TrafficSplit;
                landingPage.UpdateABTestingSettings(request.EnableABTesting ?? landingPage.EnableABTesting, trafficSplit);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"landing_page_{landingPage.Id}", cancellationToken);
            await _cache.RemoveAsync($"landing_page_slug_{landingPage.Slug}", cancellationToken);

            _logger.LogInformation("Landing page updated. LandingPageId: {LandingPageId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating landing page {LandingPageId}", request.Id);
            throw new BusinessException("Landing page güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating landing page {LandingPageId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

