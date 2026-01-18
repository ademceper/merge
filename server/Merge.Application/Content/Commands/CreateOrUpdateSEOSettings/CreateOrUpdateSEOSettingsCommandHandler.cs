using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.SEOSettings>;

namespace Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

public class CreateOrUpdateSEOSettingsCommandHandler(
    IRepository seoSettingsRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateOrUpdateSEOSettingsCommandHandler> logger) : IRequestHandler<CreateOrUpdateSEOSettingsCommand, SEOSettingsDto>
{
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";

    public async Task<SEOSettingsDto> Handle(CreateOrUpdateSEOSettingsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating or updating SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await context.Set<SEOSettings>()
                .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                        s.EntityId == request.EntityId, cancellationToken);

            SEOSettings settings;
            if (existing is not null)
            {
                logger.LogInformation("Updating existing SEO settings. SEOSettingsId: {SEOSettingsId}", existing.Id);

                existing.UpdateMetaInformation(
                    request.MetaTitle,
                    request.MetaDescription,
                    request.MetaKeywords,
                    request.CanonicalUrl);

                existing.UpdateOpenGraphInformation(
                    request.OgTitle,
                    request.OgDescription,
                    request.OgImageUrl,
                    request.TwitterCard);

                existing.UpdateStructuredData(request.StructuredDataJson);
                existing.UpdateIndexingSettings(request.IsIndexed, request.FollowLinks);
                existing.UpdateSitemapSettings(request.Priority, request.ChangeFrequency);

                settings = existing;
                await seoSettingsRepository.UpdateAsync(settings, cancellationToken);
            }
            else
            {
                logger.LogInformation("Creating new SEO settings. PageType: {PageType}", request.PageType);

                settings = SEOSettings.Create(
                    request.PageType,
                    request.EntityId,
                    request.MetaTitle,
                    request.MetaDescription,
                    request.MetaKeywords,
                    request.CanonicalUrl,
                    request.OgTitle,
                    request.OgDescription,
                    request.OgImageUrl,
                    request.TwitterCard,
                    request.StructuredDataJson,
                    request.IsIndexed,
                    request.FollowLinks,
                    request.Priority,
                    request.ChangeFrequency);

                settings = await seoSettingsRepository.AddAsync(settings, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedSettings = await context.Set<SEOSettings>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == settings.Id, cancellationToken);

            if (reloadedSettings is null)
            {
                logger.LogWarning("SEO settings {SettingsId} not found after creation/update", settings.Id);
                throw new NotFoundException("SEO Ayarları", settings.Id);
            }

            var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId?.ToString() ?? "null"}";
            await cache.RemoveAsync(cacheKey, cancellationToken);

            logger.LogInformation("SEO settings created/updated. SEOSettingsId: {SEOSettingsId}, PageType: {PageType}",
                settings.Id, request.PageType);

            return mapper.Map<SEOSettingsDto>(reloadedSettings);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating/updating SEO settings. PageType: {PageType}", request.PageType);
            throw new BusinessException("SEO ayarları oluşturma/güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating/updating SEO settings. PageType: {PageType}", request.PageType);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

