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

public class UpdateSizeGuideCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateSizeGuideCommandHandler> logger, ICacheService cache) : IRequestHandler<UpdateSizeGuideCommand, bool>
{

    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public async Task<bool> Handle(UpdateSizeGuideCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating size guide. SizeGuideId: {SizeGuideId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var sizeGuide = await context.Set<SizeGuide>()
                .Include(sg => sg.Entries)
                .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

            if (sizeGuide == null)
            {
                return false;
            }

            // Store old category ID for cache invalidation
            var oldCategoryId = sizeGuide.CategoryId;

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

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            if (oldCategoryId != request.CategoryId)
            {
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{oldCategoryId}", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);
            }

            logger.LogInformation("Size guide updated successfully. SizeGuideId: {SizeGuideId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating size guide. SizeGuideId: {SizeGuideId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
