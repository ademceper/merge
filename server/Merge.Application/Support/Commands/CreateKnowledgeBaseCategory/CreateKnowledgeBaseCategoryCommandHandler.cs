using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseCategory;

public class CreateKnowledgeBaseCategoryCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateKnowledgeBaseCategoryCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<CreateKnowledgeBaseCategoryCommand, KnowledgeBaseCategoryDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<KnowledgeBaseCategoryDto> Handle(CreateKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating knowledge base category. Name: {Name}, ParentCategoryId: {ParentCategoryId}",
            request.Name, request.ParentCategoryId);

        var slug = GenerateSlug(request.Name);

        // Ensure unique slug
        var existingSlug = await context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .AnyAsync(c => c.Slug == slug, cancellationToken);
        
        if (existingSlug)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var category = KnowledgeBaseCategory.Create(
            request.Name,
            slug,
            request.Description,
            request.ParentCategoryId,
            request.DisplayOrder,
            request.IsActive,
            request.IconUrl);

        await context.Set<KnowledgeBaseCategory>().AddAsync(category, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base category {CategoryId} created successfully. Name: {Name}, Slug: {Slug}",
            category.Id, request.Name, slug);

        category = await context.Set<KnowledgeBaseCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == category.Id, cancellationToken);

        return mapper.Map<KnowledgeBaseCategoryDto>(category!);
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
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

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (slug.Length > supportConfig.MaxCategorySlugLength)
        {
            slug = slug.Substring(0, supportConfig.MaxCategorySlugLength);
        }

        return slug;
    }
}
