using MediatR;
using Microsoft.EntityFrameworkCore;
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateProductTemplateCommandHandler : IRequestHandler<CreateProductTemplateCommand, ProductTemplateDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<CreateProductTemplateCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_ALL_TEMPLATES = "product_templates_all";
    private const string CACHE_KEY_TEMPLATES_BY_CATEGORY = "product_templates_by_category_";
    private const string CACHE_KEY_TEMPLATES_ACTIVE = "product_templates_active";
    private const string CACHE_KEY_POPULAR_TEMPLATES = "product_templates_popular_";

    public CreateProductTemplateCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<CreateProductTemplateCommandHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<ProductTemplateDto> Handle(CreateProductTemplateCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product template. Name: {Name}, CategoryId: {CategoryId}",
            request.Name, request.CategoryId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var category = await _context.Set<Category>()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException("Kategori", request.CategoryId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var template = ProductTemplate.Create(
                request.Name,
                request.Description,
                request.CategoryId,
                request.Brand,
                request.DefaultSKUPrefix,
                request.DefaultPrice,
                request.DefaultStockQuantity,
                request.DefaultImageUrl,
                request.Specifications != null ? JsonSerializer.Serialize(request.Specifications) : null,
                request.Attributes != null ? JsonSerializer.Serialize(request.Attributes) : null,
                request.IsActive);

            await _context.Set<ProductTemplate>().AddAsync(template, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            template = await _context.Set<ProductTemplate>()
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == template.Id, cancellationToken);

            _logger.LogInformation("Product template created successfully. TemplateId: {TemplateId}", template!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_TEMPLATES, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_TEMPLATES_BY_CATEGORY}{request.CategoryId}_", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_TEMPLATES_ACTIVE, cancellationToken);
            // Invalidate popular templates cache (all possible limits)
            // ✅ BOLUM 12.0: Magic number YASAK - Config kullan (ZORUNLU)
            for (int limit = _paginationSettings.DefaultPageSize; limit <= _paginationSettings.MaxPageSize; limit += _paginationSettings.DefaultPageSize)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_POPULAR_TEMPLATES}{limit}", cancellationToken);
            }

            return _mapper.Map<ProductTemplateDto>(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product template. Name: {Name}", request.Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
