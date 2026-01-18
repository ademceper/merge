using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.CreateProductTemplate;

public class CreateProductTemplateCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductTemplateCommandHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<CreateProductTemplateCommand, ProductTemplateDto>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public async Task<ProductTemplateDto> Handle(CreateProductTemplateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating product template. Name: {Name}, CategoryId: {CategoryId}",
            request.Name, request.CategoryId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await context.Set<Category>()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (category is null)
            {
                throw new NotFoundException("Kategori", request.CategoryId);
            }

            var template = ProductTemplate.Create(
                request.Name,
                request.Description,
                request.CategoryId,
                request.Brand,
                request.DefaultSKUPrefix,
                request.DefaultPrice,
                request.DefaultStockQuantity,
                request.DefaultImageUrl,
                request.Specifications is not null ? JsonSerializer.Serialize(request.Specifications) : null,
                request.Attributes is not null ? JsonSerializer.Serialize(request.Attributes) : null,
                request.IsActive);

            await context.Set<ProductTemplate>().AddAsync(template, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            template = await context.Set<ProductTemplate>()
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

            logger.LogInformation("Product template created successfully. TemplateId: {TemplateId}", template!.Id);

            await cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_TEMPLATES_ACTIVE, cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            for (int limit = paginationConfig.DefaultPageSize; limit <= paginationConfig.MaxPageSize; limit += paginationConfig.DefaultPageSize)
            {
                await cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            return mapper.Map<ProductTemplateDto>(template);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product template. Name: {Name}", request.Name);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
