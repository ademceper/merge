using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.DeleteLandingPage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteLandingPageCommandHandler : IRequestHandler<DeleteLandingPageCommand, bool>
{
    private readonly IRepository<LandingPage> _landingPageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteLandingPageCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public DeleteLandingPageCommandHandler(
        IRepository<LandingPage> landingPageRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteLandingPageCommandHandler> logger)
    {
        _landingPageRepository = landingPageRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteLandingPageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting landing page. LandingPageId: {LandingPageId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var landingPage = await _landingPageRepository.GetByIdAsync(request.Id, cancellationToken);
            if (landingPage == null)
            {
                _logger.LogWarning("Landing page not found for deletion. LandingPageId: {LandingPageId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Koruması - Manager sadece kendi landing page'lerini silebilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && landingPage.AuthorId.HasValue && landingPage.AuthorId.Value != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to delete landing page {LandingPageId} by user {UserId}. Page belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, landingPage.AuthorId.Value);
                throw new BusinessException("Bu landing page'i silme yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            landingPage.MarkAsDeleted();

            await _landingPageRepository.UpdateAsync(landingPage, cancellationToken); // Soft delete olduğu için Update
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"landing_page_{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"landing_page_slug_{landingPage.Slug}", cancellationToken);

            _logger.LogInformation("Landing page deleted (soft delete). LandingPageId: {LandingPageId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting landing page Id: {LandingPageId}", request.Id);
            throw new BusinessException("Landing page silme çakışması. Başka bir kullanıcı aynı page'i güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting landing page Id: {LandingPageId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

