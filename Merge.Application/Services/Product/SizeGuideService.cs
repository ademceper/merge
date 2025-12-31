using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Domain.Entities;
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

    public SizeGuideService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SizeGuideDto> CreateSizeGuideAsync(CreateSizeGuideDto dto)
    {
        var sizeGuide = new SizeGuide
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            Type = Enum.Parse<SizeGuideType>(dto.Type, true),
            MeasurementUnit = dto.MeasurementUnit
        };

        await _context.Set<SizeGuide>().AddAsync(sizeGuide);
        await _unitOfWork.SaveChangesAsync();

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

            await _context.Set<SizeGuideEntry>().AddAsync(entry);
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        sizeGuide = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == sizeGuide.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SizeGuideDto>(sizeGuide);
    }

    public async Task<SizeGuideDto?> GetSizeGuideAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return sizeGuide != null ? _mapper.Map<SizeGuideDto>(sizeGuide) : null;
    }

    public async Task<IEnumerable<SizeGuideDto>> GetSizeGuidesByCategoryAsync(Guid categoryId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.CategoryId == categoryId && sg.IsActive)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides);
    }

    public async Task<IEnumerable<SizeGuideDto>> GetAllSizeGuidesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuides = await _context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.IsActive)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides);
    }

    public async Task<bool> UpdateSizeGuideAsync(Guid id, CreateSizeGuideDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == id);

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

            await _context.Set<SizeGuideEntry>().AddAsync(entry);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSizeGuideAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !sg.IsDeleted (Global Query Filter)
        var sizeGuide = await _context.Set<SizeGuide>()
            .FirstOrDefaultAsync(sg => sg.Id == id);

        if (sizeGuide == null) return false;

        sizeGuide.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ProductSizeGuideDto?> GetProductSizeGuideAsync(Guid productId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.Product)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Category)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == productId);

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

    public async Task AssignSizeGuideToProductAsync(AssignSizeGuideDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !psg.IsDeleted (Global Query Filter)
        var existing = await _context.Set<ProductSizeGuide>()
            .FirstOrDefaultAsync(psg => psg.ProductId == dto.ProductId);

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

            await _context.Set<ProductSizeGuide>().AddAsync(productSizeGuide);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> RemoveSizeGuideFromProductAsync(Guid productId)
    {
        // ✅ PERFORMANCE: Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .FirstOrDefaultAsync(psg => psg.ProductId == productId);

        if (productSizeGuide == null) return false;

        productSizeGuide.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<SizeRecommendationDto> GetSizeRecommendationAsync(Guid productId, decimal height, decimal weight, decimal? chest = null, decimal? waist = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !psg.IsDeleted (Global Query Filter)
        var productSizeGuide = await _context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == productId);

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
