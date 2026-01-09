using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.DeleteSEOSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteSEOSettingsCommandHandler : IRequestHandler<DeleteSEOSettingsCommand, bool>
{
    private readonly IRepository<SEOSettings> _seoSettingsRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteSEOSettingsCommandHandler> _logger;
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";

    public DeleteSEOSettingsCommandHandler(
        IRepository<SEOSettings> seoSettingsRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteSEOSettingsCommandHandler> logger)
    {
        _seoSettingsRepository = seoSettingsRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteSEOSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var settings = await _context.Set<SEOSettings>()
                .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                        s.EntityId == request.EntityId, cancellationToken);

            if (settings == null)
            {
                _logger.LogWarning("SEO settings not found for deletion. PageType: {PageType}, EntityId: {EntityId}",
                    request.PageType, request.EntityId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            settings.MarkAsDeleted();

            await _seoSettingsRepository.UpdateAsync(settings, cancellationToken); // Soft delete olduğu için Update
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("SEO settings deleted (soft delete). SEOSettingsId: {SEOSettingsId}",
                settings.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                request.PageType, request.EntityId);
            throw new BusinessException("SEO ayarları silme çakışması. Başka bir kullanıcı aynı ayarları güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                request.PageType, request.EntityId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

