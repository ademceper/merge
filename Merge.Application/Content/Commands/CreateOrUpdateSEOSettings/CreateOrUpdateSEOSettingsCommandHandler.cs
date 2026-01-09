using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateOrUpdateSEOSettingsCommandHandler : IRequestHandler<CreateOrUpdateSEOSettingsCommand, SEOSettingsDto>
{
    private readonly IRepository<SEOSettings> _seoSettingsRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrUpdateSEOSettingsCommandHandler> _logger;
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";

    public CreateOrUpdateSEOSettingsCommandHandler(
        IRepository<SEOSettings> seoSettingsRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateOrUpdateSEOSettingsCommandHandler> logger)
    {
        _seoSettingsRepository = seoSettingsRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SEOSettingsDto> Handle(CreateOrUpdateSEOSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating or updating SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Check if exists
            var existing = await _context.Set<SEOSettings>()
                .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                        s.EntityId == request.EntityId, cancellationToken);

            SEOSettings settings;
            if (existing != null)
            {
                // Update existing
                _logger.LogInformation("Updating existing SEO settings. SEOSettingsId: {SEOSettingsId}", existing.Id);

                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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
                await _seoSettingsRepository.UpdateAsync(settings, cancellationToken);
            }
            else
            {
                // Create new
                _logger.LogInformation("Creating new SEO settings. PageType: {PageType}", request.PageType);

                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

                settings = await _seoSettingsRepository.AddAsync(settings, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with AsNoTracking
            var reloadedSettings = await _context.Set<SEOSettings>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == settings.Id, cancellationToken);

            if (reloadedSettings == null)
            {
                _logger.LogWarning("SEO settings {SettingsId} not found after creation/update", settings.Id);
                throw new NotFoundException("SEO Ayarları", settings.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId?.ToString() ?? "null"}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);

            _logger.LogInformation("SEO settings created/updated. SEOSettingsId: {SEOSettingsId}, PageType: {PageType}",
                settings.Id, request.PageType);

            return _mapper.Map<SEOSettingsDto>(reloadedSettings);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating/updating SEO settings. PageType: {PageType}", request.PageType);
            throw new BusinessException("SEO ayarları oluşturma/güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating/updating SEO settings. PageType: {PageType}", request.PageType);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

