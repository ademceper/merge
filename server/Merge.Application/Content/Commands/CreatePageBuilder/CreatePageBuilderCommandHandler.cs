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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.PageBuilder>;

namespace Merge.Application.Content.Commands.CreatePageBuilder;

public class CreatePageBuilderCommandHandler(
    IRepository pageBuilderRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreatePageBuilderCommandHandler> logger) : IRequestHandler<CreatePageBuilderCommand, PageBuilderDto>
{
    private const string CACHE_KEY_ALL_PAGES = "page_builders_all";
    private const string CACHE_KEY_ACTIVE_PAGES = "page_builders_active";

    public async Task<PageBuilderDto> Handle(CreatePageBuilderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating page builder. AuthorId: {AuthorId}, Name: {Name}, Title: {Title}",
            request.AuthorId, request.Name, request.Title);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Slug uniqueness check
            var slug = request.Slug ?? GenerateSlug(request.Name);
            if (await context.Set<PageBuilder>().AnyAsync(pb => pb.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            var pageBuilder = PageBuilder.Create(
                name: request.Name,
                title: request.Title,
                content: request.Content,
                authorId: request.AuthorId,
                slug: slug,
                template: request.Template,
                status: ContentStatus.Draft,
                pageType: request.PageType,
                relatedEntityId: request.RelatedEntityId,
                metaTitle: request.MetaTitle,
                metaDescription: request.MetaDescription,
                ogImageUrl: request.OgImageUrl);

            pageBuilder = await pageBuilderRepository.AddAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedPage = await context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Id == pageBuilder.Id, cancellationToken);

            if (reloadedPage == null)
            {
                logger.LogWarning("Page builder {PageBuilderId} not found after creation", pageBuilder.Id);
                throw new NotFoundException("Page Builder", pageBuilder.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_PAGES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_PAGES, cancellationToken);
            await cache.RemoveAsync($"page_builder_{pageBuilder.Id}", cancellationToken);
            await cache.RemoveAsync($"page_builder_slug_{pageBuilder.Slug}", cancellationToken);

            logger.LogInformation("Page builder created. PageBuilderId: {PageBuilderId}, Slug: {Slug}, AuthorId: {AuthorId}",
                pageBuilder.Id, pageBuilder.Slug, request.AuthorId);

            return mapper.Map<PageBuilderDto>(reloadedPage);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating page builder with Name: {Name}", request.Name);
            throw new BusinessException("Page builder oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating page builder with Name: {Name}", request.Name);
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

