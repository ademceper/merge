using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseCategory;

public class UpdateKnowledgeBaseCategoryCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateKnowledgeBaseCategoryCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<UpdateKnowledgeBaseCategoryCommand, KnowledgeBaseCategoryDto?>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<KnowledgeBaseCategoryDto?> Handle(UpdateKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating knowledge base category {CategoryId}", request.CategoryId);

        var category = await context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            logger.LogWarning("Knowledge base category {CategoryId} not found for update", request.CategoryId);
            throw new NotFoundException("Bilgi bankası kategorisi", request.CategoryId);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            var newSlug = GenerateSlug(request.Name);
            category.UpdateName(request.Name, newSlug);
        }
        if (request.Description != null)
        {
            category.UpdateDescription(request.Description);
        }
        if (request.ParentCategoryId.HasValue)
        {
            category.UpdateParentCategory(request.ParentCategoryId.Value);
        }
        if (request.DisplayOrder.HasValue)
        {
            category.UpdateDisplayOrder(request.DisplayOrder.Value);
        }
        if (request.IsActive.HasValue)
        {
            category.SetActive(request.IsActive.Value);
        }
        if (request.IconUrl != null)
        {
            category.UpdateIconUrl(request.IconUrl);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base category {CategoryId} updated successfully", request.CategoryId);

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
