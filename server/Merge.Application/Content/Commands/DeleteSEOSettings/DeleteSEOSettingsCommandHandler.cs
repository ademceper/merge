using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.DeleteSEOSettings;

public class DeleteSEOSettingsCommandHandler(
    Merge.Application.Interfaces.IRepository<SEOSettings> seoSettingsRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteSEOSettingsCommandHandler> logger) : IRequestHandler<DeleteSEOSettingsCommand, bool>
{
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";

    public async Task<bool> Handle(DeleteSEOSettingsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var settings = await context.Set<SEOSettings>()
                .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                        s.EntityId == request.EntityId, cancellationToken);

            if (settings == null)
            {
                logger.LogWarning("SEO settings not found for deletion. PageType: {PageType}, EntityId: {EntityId}",
                    request.PageType, request.EntityId);
                return false;
            }

            settings.MarkAsDeleted();

            await seoSettingsRepository.UpdateAsync(settings, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId}";
            await cache.RemoveAsync(cacheKey, cancellationToken);

            logger.LogInformation("SEO settings deleted (soft delete). SEOSettingsId: {SEOSettingsId}",
                settings.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                request.PageType, request.EntityId);
            throw new BusinessException("SEO ayarları silme çakışması. Başka bir kullanıcı aynı ayarları güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                request.PageType, request.EntityId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

