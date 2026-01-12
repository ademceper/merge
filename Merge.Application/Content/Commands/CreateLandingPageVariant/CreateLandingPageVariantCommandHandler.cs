using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.CreateLandingPageVariant;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateLandingPageVariantCommandHandler : IRequestHandler<CreateLandingPageVariantCommand, LandingPageDto>
{
    private readonly Merge.Application.Interfaces.IRepository<LandingPage> _landingPageRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLandingPageVariantCommandHandler> _logger;
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public CreateLandingPageVariantCommandHandler(
        Merge.Application.Interfaces.IRepository<LandingPage> landingPageRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateLandingPageVariantCommandHandler> logger)
    {
        _landingPageRepository = landingPageRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LandingPageDto> Handle(CreateLandingPageVariantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating landing page variant. OriginalId: {OriginalId}, Name: {Name}", request.OriginalId, request.Name);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var original = await _context.Set<LandingPage>()
                .FirstOrDefaultAsync(lp => lp.Id == request.OriginalId, cancellationToken);

            if (original == null)
            {
                _logger.LogWarning("Original landing page not found. OriginalId: {OriginalId}", request.OriginalId);
                throw new NotFoundException("Orijinal landing page", request.OriginalId);
            }

            if (!original.EnableABTesting)
            {
                _logger.LogWarning("A/B testing not enabled for landing page. OriginalId: {OriginalId}", request.OriginalId);
                throw new BusinessException("A/B testi etkinleştirilmemiş landing page için variant oluşturulamaz");
            }

            // Parse status enum
            if (!Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
            {
                statusEnum = ContentStatus.Draft;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            var variant = original.CreateVariant(
                name: request.Name,
                title: request.Title,
                content: request.Content,
                template: request.Template ?? original.Template,
                status: statusEnum,
                startDate: request.StartDate ?? original.StartDate,
                endDate: request.EndDate ?? original.EndDate,
                metaTitle: request.MetaTitle ?? original.MetaTitle,
                metaDescription: request.MetaDescription ?? original.MetaDescription,
                ogImageUrl: request.OgImageUrl ?? original.OgImageUrl,
                trafficSplit: request.TrafficSplit);

            variant = await _landingPageRepository.AddAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedVariant = await _context.Set<LandingPage>()
                .AsNoTracking()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == variant.Id, cancellationToken);

            if (reloadedVariant == null)
            {
                _logger.LogWarning("Landing page variant {VariantId} not found after creation", variant.Id);
                throw new NotFoundException("Landing Page Variant", variant.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await _cache.RemoveAsync($"landing_page_{request.OriginalId}", cancellationToken);
            await _cache.RemoveAsync($"landing_page_{variant.Id}", cancellationToken);

            _logger.LogInformation("Landing page variant created. VariantId: {VariantId}, OriginalId: {OriginalId}", variant.Id, request.OriginalId);

            return _mapper.Map<LandingPageDto>(reloadedVariant);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating landing page variant for OriginalId: {OriginalId}", request.OriginalId);
            throw new BusinessException("Landing page variant oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating landing page variant for OriginalId: {OriginalId}", request.OriginalId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

