using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.GenerateCategorySEO;

public class GenerateCategorySEOCommandHandler(
    IDbContext context,
    IMediator mediator,
    IMapper mapper,
    ILogger<GenerateCategorySEOCommandHandler> logger) : IRequestHandler<GenerateCategorySEOCommand, SEOSettingsDto>
{

    public async Task<SEOSettingsDto> Handle(GenerateCategorySEOCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating SEO for category. CategoryId: {CategoryId}", request.CategoryId);

        var category = await context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Category not found. CategoryId: {CategoryId}", request.CategoryId);
            throw new NotFoundException("Kategori", request.CategoryId);
        }

        var metaTitle = $"{category.Name} - Shop Online";
        var metaDescription = !string.IsNullOrEmpty(category.Description)
            ? category.Description.Length > 160
                ? category.Description.Substring(0, 157) + "..."
                : category.Description
            : $"Browse {category.Name} products. Wide selection and best prices.";

        var command = new CreateOrUpdateSEOSettingsCommand(
            PageType: "Category",
            EntityId: request.CategoryId,
            MetaTitle: metaTitle,
            MetaDescription: metaDescription,
            MetaKeywords: category.Name,
            CanonicalUrl: $"/categories/{category.Slug}",
            IsIndexed: true,
            FollowLinks: true,
            Priority: 0.7m,
            ChangeFrequency: "daily");

        return await mediator.Send(command, cancellationToken);
    }
}

