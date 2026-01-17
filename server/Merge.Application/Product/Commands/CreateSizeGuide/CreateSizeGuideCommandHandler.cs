using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using AutoMapper;
namespace Merge.Application.Product.Commands.CreateSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateSizeGuideCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CreateSizeGuideCommandHandler> logger,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<CreateSizeGuideCommand, SizeGuideDto>
{

    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public async Task<SizeGuideDto> Handle(CreateSizeGuideCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating size guide. Name: {Name}, CategoryId: {CategoryId}",
            request.Name, request.CategoryId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var sizeGuide = SizeGuide.Create(
                request.Name,
                request.Description,
                request.CategoryId,
                Enum.Parse<SizeGuideType>(request.Type, true),
                request.Brand,
                request.MeasurementUnit);

            await context.Set<SizeGuide>().AddAsync(sizeGuide, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            sizeGuide = await context.Set<SizeGuide>()
                .AsNoTracking()
                .Include(sg => sg.Category)
                .Include(sg => sg.Entries)
                .FirstOrDefaultAsync(sg => sg.Id == sizeGuide.Id, cancellationToken);

            logger.LogInformation("Size guide created successfully. SizeGuideId: {SizeGuideId}", sizeGuide!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            await cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            await cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);

            return mapper.Map<SizeGuideDto>(sizeGuide);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating size guide. Name: {Name}", request.Name);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
