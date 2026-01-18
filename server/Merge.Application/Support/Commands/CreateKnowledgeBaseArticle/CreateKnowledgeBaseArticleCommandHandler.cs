using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;

public class CreateKnowledgeBaseArticleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateKnowledgeBaseArticleCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<CreateKnowledgeBaseArticleCommand, KnowledgeBaseArticleDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<KnowledgeBaseArticleDto> Handle(CreateKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating knowledge base article. Title: {Title}, AuthorId: {AuthorId}, Status: {Status}",
            request.Title, request.AuthorId, request.Status);

        var slug = GenerateSlug(request.Title);

        // Ensure unique slug
        var existingSlug = await context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AnyAsync(a => a.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var article = KnowledgeBaseArticle.Create(
            request.Title,
            slug,
            request.Content,
            request.AuthorId,
            request.CategoryId,
            request.Excerpt,
            Enum.Parse<ContentStatus>(request.Status, true),
            request.IsFeatured,
            request.DisplayOrder,
            request.Tags is not null ? string.Join(",", request.Tags) : null);

        await context.Set<KnowledgeBaseArticle>().AddAsync(article, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base article {ArticleId} created successfully. Title: {Title}, Slug: {Slug}",
            article.Id, request.Title, slug);

        article = await context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == article.Id, cancellationToken);

        return mapper.Map<KnowledgeBaseArticleDto>(article!);
    }

    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("Ğ", "g")
            .Replace("Ü", "u")
            .Replace("Ş", "s")
            .Replace("İ", "i")
            .Replace("Ö", "o")
            .Replace("Ç", "c");

        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Remove leading/trailing dashes
        slug = slug.Trim('-');

        if (slug.Length > supportConfig.MaxArticleSlugLength)
        {
            slug = slug.Substring(0, supportConfig.MaxArticleSlugLength);
        }

        return slug;
    }
}
