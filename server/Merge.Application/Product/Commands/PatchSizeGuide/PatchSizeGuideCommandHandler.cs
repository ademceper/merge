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

namespace Merge.Application.Product.Commands.PatchSizeGuide;

/// <summary>
/// Handler for PatchSizeGuideCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchSizeGuideCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PatchSizeGuideCommandHandler> logger,
    ICacheService cache) : IRequestHandler<PatchSizeGuideCommand, bool>
{
    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public async Task<bool> Handle(PatchSizeGuideCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching size guide. SizeGuideId: {SizeGuideId}", request.Id);

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

            var oldCategoryId = sizeGuide.CategoryId;

            // Apply partial updates
            var name = request.PatchDto.Name ?? sizeGuide.Name;
            var description = request.PatchDto.Description ?? sizeGuide.Description;
            var categoryId = request.PatchDto.CategoryId ?? sizeGuide.CategoryId;
            var type = request.PatchDto.Type != null ? Enum.Parse<SizeGuideType>(request.PatchDto.Type, true) : sizeGuide.Type;
            var brand = request.PatchDto.Brand ?? sizeGuide.Brand;
            var measurementUnit = request.PatchDto.MeasurementUnit ?? sizeGuide.MeasurementUnit;

            sizeGuide.Update(name, description, categoryId, type, brand, measurementUnit);

            if (request.PatchDto.Entries != null)
            {
                var entryIds = sizeGuide.Entries.Select(e => e.Id).ToList();
                foreach (var entryId in entryIds)
                {
                    sizeGuide.RemoveEntry(entryId);
                }

                foreach (var entryDto in request.PatchDto.Entries)
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
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}", cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            if (oldCategoryId != categoryId)
            {
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{oldCategoryId}", cancellationToken);
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{categoryId}", cancellationToken);
            }
            else
            {
                await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{categoryId}", cancellationToken);
            }

            logger.LogInformation("Size guide patched successfully. SizeGuideId: {SizeGuideId}", request.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error patching size guide. SizeGuideId: {SizeGuideId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
