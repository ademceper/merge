using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Product;

public class SizeGuideService : ISizeGuideService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SizeGuideService> _logger;

    public SizeGuideService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SizeGuideService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<SizeGuideDto> CreateSizeGuideAsync(CreateSizeGuideDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Size guide oluşturuluyor. Name: {Name}, CategoryId: {CategoryId}",
            dto.Name, dto.CategoryId);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var sizeGuide = SizeGuide.Create(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            Enum.Parse<SizeGuideType>(dto.Type, true),
            dto.Brand,
            dto.MeasurementUnit);

        await _context.Set<SizeGuide>().AddAsync(sizeGuide, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method ve Domain Method kullanımı
        foreach (var entryDto in dto.Entries)
        {
            var additionalMeasurementsJson = entryDto.AdditionalMeasurements != null 
                ? JsonSerializer.Serialize(entryDto.AdditionalMeasurements) 
                : null;

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
                additionalMeasurementsJson,
                entryDto.DisplayOrder);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            sizeGuide.AddEntry(entry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        sizeGuide = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == sizeGuide.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Size guide oluşturuldu. SizeGuideId: {SizeGuideId}, Name: {Name}",
            sizeGuide!.Id, sizeGuide.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SizeGuideDto>(sizeGuide);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SizeGuideDto?> GetSizeGuideAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return sizeGuide != null ? _mapper.Map<SizeGuideDto>(sizeGuide) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SizeGuideDto>> GetSizeGuidesByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.CategoryId == categoryId && sg.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SizeGuideDto>> GetAllSizeGuidesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateSizeGuideAsync(Guid id, CreateSizeGuideDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == id, cancellationToken);

        if (sizeGuide == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        sizeGuide.Update(
            dto.Name,
            dto.Description,
            dto.CategoryId,
            Enum.Parse<SizeGuideType>(dto.Type, true),
            dto.Brand,
            dto.MeasurementUnit);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        // Remove old entries
        var entryIdsToRemove = sizeGuide.Entries.Select(e => e.Id).ToList();
        foreach (var entryId in entryIdsToRemove)
        {
            sizeGuide.RemoveEntry(entryId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method ve Domain Method kullanımı
        // Add new entries
        foreach (var entryDto in dto.Entries)
        {
            var additionalMeasurementsJson = entryDto.AdditionalMeasurements != null 
                ? JsonSerializer.Serialize(entryDto.AdditionalMeasurements) 
                : null;

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
                additionalMeasurementsJson,
                entryDto.DisplayOrder);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            sizeGuide.AddEntry(entry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteSizeGuideAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .FirstOrDefaultAsync(sg => sg.Id == id, cancellationToken);

        if (sizeGuide == null) return false;

        sizeGuide.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductSizeGuideDto?> GetProductSizeGuideAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.Product)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Category)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == productId, cancellationToken);

        if (productSizeGuide == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var sizeGuideDto = _mapper.Map<SizeGuideDto>(productSizeGuide.SizeGuide);
        return new ProductSizeGuideDto(
            productSizeGuide.ProductId,
            productSizeGuide.Product.Name,
            sizeGuideDto,
            productSizeGuide.CustomNotes,
            productSizeGuide.FitDescription);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task AssignSizeGuideToProductAsync(AssignSizeGuideDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !psg.IsDeleted (Global Query Filter)
        var existing = await _context.Set<ProductSizeGuide>()
            .FirstOrDefaultAsync(psg => psg.ProductId == dto.ProductId, cancellationToken);

        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Update(
                dto.SizeGuideId,
                dto.CustomNotes,
                null, // fitType değiştirilmiyor
                dto.FitDescription);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var productSizeGuide = ProductSizeGuide.Create(
                dto.ProductId,
                dto.SizeGuideId,
                dto.CustomNotes,
                true, // default fitType
                dto.FitDescription);

            await _context.Set<ProductSizeGuide>().AddAsync(productSizeGuide, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RemoveSizeGuideFromProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .FirstOrDefaultAsync(psg => psg.ProductId == productId, cancellationToken);

        if (productSizeGuide == null) return false;

        productSizeGuide.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SizeRecommendationDto> GetSizeRecommendationAsync(Guid productId, decimal height, decimal weight, decimal? chest = null, decimal? waist = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == productId, cancellationToken);

        if (productSizeGuide == null)
        {
            return new SizeRecommendationDto(
                RecommendedSize: "N/A",
                Confidence: "Low",
                AlternativeSizes: Array.Empty<string>().ToList().AsReadOnly(),
                Reasoning: "No size guide available for this product");
        }

        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var entries = productSizeGuide.SizeGuide.Entries
            .OrderBy(e => e.DisplayOrder)
            .ToList();

        // Simple recommendation logic based on measurements
        SizeGuideEntry? bestMatch = null;
        decimal bestScore = decimal.MaxValue;

        foreach (var entry in entries)
        {
            decimal score = 0;
            int matchCount = 0;

            if (entry.Height.HasValue)
            {
                score += Math.Abs(entry.Height.Value - height);
                matchCount++;
            }

            if (entry.Weight.HasValue)
            {
                score += Math.Abs(entry.Weight.Value - weight);
                matchCount++;
            }

            if (chest.HasValue && entry.Chest.HasValue)
            {
                score += Math.Abs(entry.Chest.Value - chest.Value);
                matchCount++;
            }

            if (waist.HasValue && entry.Waist.HasValue)
            {
                score += Math.Abs(entry.Waist.Value - waist.Value);
                matchCount++;
            }

            if (matchCount > 0)
            {
                score /= matchCount;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestMatch = entry;
                }
            }
        }

        if (bestMatch != null)
        {
            var currentIndex = entries.IndexOf(bestMatch);
            var alternatives = new List<string>();

            if (currentIndex > 0)
                alternatives.Add(entries[currentIndex - 1].SizeLabel);
            if (currentIndex < entries.Count - 1)
                alternatives.Add(entries[currentIndex + 1].SizeLabel);

            return new SizeRecommendationDto(
                RecommendedSize: bestMatch.SizeLabel,
                Confidence: bestScore < 5 ? "High" : bestScore < 15 ? "Medium" : "Low",
                AlternativeSizes: alternatives.AsReadOnly(),
                Reasoning: $"Based on your measurements (Height: {height}cm, Weight: {weight}kg)");
        }

        return new SizeRecommendationDto(
            RecommendedSize: "N/A",
            Confidence: "Low",
            AlternativeSizes: Array.Empty<string>().ToList().AsReadOnly(),
            Reasoning: "Unable to determine size based on provided measurements");
    }

}
