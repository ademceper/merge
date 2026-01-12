using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Commands.UpdateSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateSizeGuideCommandHandler : IRequestHandler<UpdateSizeGuideCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSizeGuideCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public UpdateSizeGuideCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSizeGuideCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateSizeGuideCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating size guide. SizeGuideId: {SizeGuideId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sizeGuide = await _context.Set<SizeGuide>()
                .Include(sg => sg.Entries)
                .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

            if (sizeGuide == null)
            {
                return false;
            }

            // Store old category ID for cache invalidation
            var oldCategoryId = sizeGuide.CategoryId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            sizeGuide.Update(
                request.Name,
                request.Description,
                request.CategoryId,
                Enum.Parse<SizeGuideType>(request.Type, true),
                request.Brand,
                request.MeasurementUnit);

            // Remove old entries (soft delete)
            var entryIds = sizeGuide.Entries.Select(e => e.Id).ToList();
            foreach (var entryId in entryIds)
            {
                sizeGuide.RemoveEntry(entryId);
            }

            // Add new entries
            foreach (var entryDto in request.Entries)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
                var entry = SizeGuideEntry.Create(
                    sizeGuide.Id,
                    entryDto.SizeLabel,
                    entryDto.AlternativeLabel,
                    entryDto.Chest,
                    entryDto.Waist,
                    entryDto.Hips,
                    entryDto.Inseam,
                    entryDto.Shoulder,
                    entryDto.Length,
                    entryDto.Width,
                    entryDto.Height,
                    entryDto.Weight,
                    entryDto.AdditionalMeasurements != null ? JsonSerializer.Serialize(entryDto.AdditionalMeasurements) : null,
                    entryDto.DisplayOrder);

                sizeGuide.AddEntry(entry);
            }

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}", cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            if (oldCategoryId != request.CategoryId)
            {
                await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{oldCategoryId}", cancellationToken);
                await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);
            }
            else
            {
                await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);
            }

            _logger.LogInformation("Size guide updated successfully. SizeGuideId: {SizeGuideId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating size guide. SizeGuideId: {SizeGuideId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
