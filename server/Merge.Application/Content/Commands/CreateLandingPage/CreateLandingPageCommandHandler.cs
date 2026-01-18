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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.LandingPage>;

namespace Merge.Application.Content.Commands.CreateLandingPage;

public class CreateLandingPageCommandHandler(
    IRepository landingPageRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateLandingPageCommandHandler> logger) : IRequestHandler<CreateLandingPageCommand, LandingPageDto>
{
    private const string CACHE_KEY_ALL_PAGES = "landing_pages_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "landing_pages_active";

    public async Task<LandingPageDto> Handle(CreateLandingPageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating landing page. AuthorId: {AuthorId}, Name: {Name}, Title: {Title}",
            request.AuthorId, request.Name, request.Title);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Variant validation (if creating a variant)
            if (request.VariantOfId.HasValue)
            {
                var original = await context.Set<LandingPage>()
                    .FirstOrDefaultAsync(lp => lp.Id == request.VariantOfId.Value, cancellationToken);

                if (original is null)
                {
                    logger.LogWarning("Landing page variant creation failed: Original not found. VariantOfId: {VariantOfId}", request.VariantOfId);
                    throw new NotFoundException("Orijinal landing page", request.VariantOfId.Value);
                }

                if (!original.EnableABTesting)
                {
                    logger.LogWarning("Landing page variant creation failed: A/B testing not enabled. VariantOfId: {VariantOfId}", request.VariantOfId);
                    throw new BusinessException("A/B testi etkinleştirilmemiş landing page için variant oluşturulamaz");
                }
            }

            // Slug uniqueness check
            var slug = GenerateSlug(request.Name);
            if (await context.Set<LandingPage>().AnyAsync(lp => lp.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // Parse status enum
            if (!Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
            {
                statusEnum = ContentStatus.Draft;
            }

            LandingPage landingPage;
            if (request.VariantOfId.HasValue)
            {
                var original = await context.Set<LandingPage>()
                    .FirstOrDefaultAsync(lp => lp.Id == request.VariantOfId.Value, cancellationToken);

                if (original is null)
                {
                    throw new NotFoundException("Orijinal landing page", request.VariantOfId.Value);
                }

                landingPage = original.CreateVariant(
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
            }
            else
            {
                landingPage = LandingPage.Create(
                    name: request.Name,
                    title: request.Title,
                    content: request.Content,
                    authorId: request.AuthorId,
                    template: request.Template,
                    status: statusEnum,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    metaTitle: request.MetaTitle,
                    metaDescription: request.MetaDescription,
                    ogImageUrl: request.OgImageUrl,
                    enableABTesting: request.EnableABTesting,
                    variantOfId: request.VariantOfId,
                    trafficSplit: request.TrafficSplit,
                    slug: slug);
            }

            landingPage = await landingPageRepository.AddAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedPage = await context.Set<LandingPage>()
                .AsNoTracking()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == landingPage.Id, cancellationToken);

            if (reloadedPage is null)
            {
                logger.LogWarning("Landing page {LandingPageId} not found after creation", landingPage.Id);
                throw new NotFoundException("Landing Page", landingPage.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"landing_page_{landingPage.Id}", cancellationToken); // Single page cache
            await cache.RemoveAsync($"landing_page_slug_{landingPage.Slug}", cancellationToken); // Slug cache

            logger.LogInformation("Landing page created. LandingPageId: {LandingPageId}, Slug: {Slug}, AuthorId: {AuthorId}",
                landingPage.Id, landingPage.Slug, request.AuthorId);

            return mapper.Map<LandingPageDto>(reloadedPage);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating landing page with Name: {Name}", request.Name);
            throw new BusinessException("Landing page oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating landing page with Name: {Name}", request.Name);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }
}

