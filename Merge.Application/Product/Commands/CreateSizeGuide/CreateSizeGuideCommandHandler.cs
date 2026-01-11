using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;

namespace Merge.Application.Product.Commands.CreateSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateSizeGuideCommandHandler : IRequestHandler<CreateSizeGuideCommand, SizeGuideDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<CreateSizeGuideCommandHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";
    private const string CACHE_KEY_SIZE_GUIDES_BY_CATEGORY = "size_guides_by_category_";

    public CreateSizeGuideCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        AutoMapper.IMapper mapper,
        ILogger<CreateSizeGuideCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SizeGuideDto> Handle(CreateSizeGuideCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating size guide. Name: {Name}, CategoryId: {CategoryId}",
            request.Name, request.CategoryId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
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

            await _context.Set<SizeGuide>().AddAsync(sizeGuide, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            sizeGuide = await _context.Set<SizeGuide>()
                .AsNoTracking()
                .Include(sg => sg.Category)
                .Include(sg => sg.Entries)
                .FirstOrDefaultAsync(sg => sg.Id == sizeGuide.Id, cancellationToken);

            _logger.LogInformation("Size guide created successfully. SizeGuideId: {SizeGuideId}", sizeGuide!.Id);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
            await _cache.RemoveAsync($"{CACHE_KEY_SIZE_GUIDES_BY_CATEGORY}{request.CategoryId}", cancellationToken);

            return _mapper.Map<SizeGuideDto>(sizeGuide);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating size guide. Name: {Name}", request.Name);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
