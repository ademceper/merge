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

public class CreateLandingPageVariantCommandHandler(
    Merge.Application.Interfaces.IRepository<LandingPage> landingPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateLandingPageVariantCommandHandler> logger) : IRequestHandler<CreateLandingPageVariantCommand, LandingPageDto>
{
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public async Task<LandingPageDto> Handle(CreateLandingPageVariantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating landing page variant. OriginalId: {OriginalId}, Name: {Name}", request.OriginalId, request.Name);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var original = await context.Set<LandingPage>()
                .FirstOrDefaultAsync(lp => lp.Id == request.OriginalId, cancellationToken);

            if (original == null)
            {
                logger.LogWarning("Original landing page not found. OriginalId: {OriginalId}", request.OriginalId);
                throw new NotFoundException("Orijinal landing page", request.OriginalId);
            }

            if (!original.EnableABTesting)
            {
                logger.LogWarning("A/B testing not enabled for landing page. OriginalId: {OriginalId}", request.OriginalId);
                throw new BusinessException("A/B testi etkinleştirilmemiş landing page için variant oluşturulamaz");
            }

            if (!Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
            {
                statusEnum = ContentStatus.Draft;
            }

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

            variant = await landingPageRepository.AddAsync(variant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedVariant = await context.Set<LandingPage>()
                .AsNoTracking()
            .AsSplitQuery()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == variant.Id, cancellationToken);

            if (reloadedVariant == null)
            {
                logger.LogWarning("Landing page variant {VariantId} not found after creation", variant.Id);
                throw new NotFoundException("Landing Page Variant", variant.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"landing_page_{request.OriginalId}", cancellationToken);
            await cache.RemoveAsync($"landing_page_{variant.Id}", cancellationToken);

            logger.LogInformation("Landing page variant created. VariantId: {VariantId}, OriginalId: {OriginalId}", variant.Id, request.OriginalId);

            return mapper.Map<LandingPageDto>(reloadedVariant);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating landing page variant for OriginalId: {OriginalId}", request.OriginalId);
            throw new BusinessException("Landing page variant oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating landing page variant for OriginalId: {OriginalId}", request.OriginalId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

