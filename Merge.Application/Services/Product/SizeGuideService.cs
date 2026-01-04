using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Product;

public class SizeGuideService : ISizeGuideService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SizeGuideService> _logger;

    public SizeGuideService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SizeGuideService> logger)
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

        var sizeGuide = new SizeGuide
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            Type = Enum.Parse<SizeGuideType>(dto.Type, true),
            MeasurementUnit = dto.MeasurementUnit
        };

        await _context.Set<SizeGuide>().AddAsync(sizeGuide, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entryDto in dto.Entries)
        {
            var entry = new SizeGuideEntry
            {
                SizeGuideId = sizeGuide.Id,
                SizeLabel = entryDto.SizeLabel,
                AlternativeLabel = entryDto.AlternativeLabel,
                Chest = entryDto.Chest,
                Waist = entryDto.Waist,
                Hips = entryDto.Hips,
                Inseam = entryDto.Inseam,
                Shoulder = entryDto.Shoulder,
                Length = entryDto.Length,
                Width = entryDto.Width,
                Height = entryDto.Height,
                Weight = entryDto.Weight,
                AdditionalMeasurements = entryDto.AdditionalMeasurements != null ? JsonSerializer.Serialize(entryDto.AdditionalMeasurements) : null,
                DisplayOrder = entryDto.DisplayOrder
            };

            await _context.Set<SizeGuideEntry>().AddAsync(entry, cancellationToken);
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

        sizeGuide.Name = dto.Name;
        sizeGuide.Description = dto.Description;
        sizeGuide.CategoryId = dto.CategoryId;
        sizeGuide.Brand = dto.Brand;
        sizeGuide.Type = Enum.Parse<SizeGuideType>(dto.Type, true);
        sizeGuide.MeasurementUnit = dto.MeasurementUnit;

        // Remove old entries
        foreach (var entry in sizeGuide.Entries)
        {
            entry.IsDeleted = true;
        }

        // Add new entries
        foreach (var entryDto in dto.Entries)
        {
            var entry = new SizeGuideEntry
            {
                SizeGuideId = sizeGuide.Id,
                SizeLabel = entryDto.SizeLabel,
                AlternativeLabel = entryDto.AlternativeLabel,
                Chest = entryDto.Chest,
                Waist = entryDto.Waist,
                Hips = entryDto.Hips,
                Inseam = entryDto.Inseam,
                Shoulder = entryDto.Shoulder,
                Length = entryDto.Length,
                Width = entryDto.Width,
                Height = entryDto.Height,
                Weight = entryDto.Weight,
                AdditionalMeasurements = entryDto.AdditionalMeasurements != null ? JsonSerializer.Serialize(entryDto.AdditionalMeasurements) : null,
                DisplayOrder = entryDto.DisplayOrder
            };

            await _context.Set<SizeGuideEntry>().AddAsync(entry, cancellationToken);
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
        return new ProductSizeGuideDto
        {
            ProductId = productSizeGuide.ProductId,
            ProductName = productSizeGuide.Product.Name,
            SizeGuide = sizeGuideDto,
            CustomNotes = productSizeGuide.CustomNotes,
            FitDescription = productSizeGuide.FitDescription
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task AssignSizeGuideToProductAsync(AssignSizeGuideDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !psg.IsDeleted (Global Query Filter)
        var existing = await _context.Set<ProductSizeGuide>()
            .FirstOrDefaultAsync(psg => psg.ProductId == dto.ProductId, cancellationToken);

        if (existing != null)
        {
            existing.SizeGuideId = dto.SizeGuideId;
            existing.CustomNotes = dto.CustomNotes;
            existing.FitDescription = dto.FitDescription;
        }
        else
        {
            var productSizeGuide = new ProductSizeGuide
            {
                ProductId = dto.ProductId,
                SizeGuideId = dto.SizeGuideId,
                CustomNotes = dto.CustomNotes,
                FitDescription = dto.FitDescription
            };

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
            return new SizeRecommendationDto
            {
                RecommendedSize = "N/A",
                Confidence = "Low",
                Reasoning = "No size guide available for this product"
            };
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

            return new SizeRecommendationDto
            {
                RecommendedSize = bestMatch.SizeLabel,
                Confidence = bestScore < 5 ? "High" : bestScore < 15 ? "Medium" : "Low",
                AlternativeSizes = alternatives,
                Reasoning = $"Based on your measurements (Height: {height}cm, Weight: {weight}kg)"
            };
        }

        return new SizeRecommendationDto
        {
            RecommendedSize = "N/A",
            Confidence = "Low",
            Reasoning = "Unable to determine size based on provided measurements"
        };
    }

}
