using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.International;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.International;


namespace Merge.Application.Services.International;

public class LanguageService : ILanguageService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LanguageService> _logger;

    public LanguageService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LanguageService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Language Management

    public async Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var languages = await _context.Set<Language>()
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<LanguageDto>>(languages);
    }

    public async Task<IEnumerable<LanguageDto>> GetActiveLanguagesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var languages = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<LanguageDto>>(languages);
    }

    public async Task<LanguageDto?> GetLanguageByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }

    public async Task<LanguageDto?> GetLanguageByCodeAsync(string code)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == code.ToLower());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }

    public async Task<LanguageDto> CreateLanguageAsync(CreateLanguageDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            throw new ValidationException("Dil kodu boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Dil adı boş olamaz.");
        }

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var exists = await _context.Set<Language>()
            .AnyAsync(l => l.Code.ToLower() == dto.Code.ToLower());

        if (exists)
        {
            throw new BusinessException($"Bu dil kodu zaten mevcut: {dto.Code}");
        }

        if (dto.IsDefault)
        {
            // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
            var currentDefault = await _context.Set<Language>()
                .FirstOrDefaultAsync(l => l.IsDefault);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
            }
        }

        var language = new Language
        {
            Code = dto.Code.ToLower(),
            Name = dto.Name,
            NativeName = dto.NativeName,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            IsRTL = dto.IsRTL,
            FlagIcon = dto.FlagIcon
        };

        await _context.Set<Language>().AddAsync(language);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LanguageDto>(language);
    }

    public async Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (language == null)
        {
            throw new NotFoundException("Dil", id);
        }

        language.Name = dto.Name;
        language.NativeName = dto.NativeName;
        language.IsActive = dto.IsActive;
        language.IsRTL = dto.IsRTL;
        language.FlagIcon = dto.FlagIcon;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LanguageDto>(language);
    }

    public async Task DeleteLanguageAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == id);

        if (language == null)
        {
            throw new NotFoundException("Dil", id);
        }

        if (language.IsDefault)
        {
            throw new BusinessException("Varsayılan dil silinemez.");
        }

        language.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Product Translations

    public async Task<ProductTranslationDto> CreateProductTranslationAsync(CreateProductTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower());

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var exists = await _context.Set<ProductTranslation>()
            .AnyAsync(pt => pt.ProductId == dto.ProductId &&
                           pt.LanguageCode.ToLower() == dto.LanguageCode.ToLower());

        if (exists)
        {
            throw new BusinessException("Bu ürün ve dil için çeviri zaten mevcut.");
        }

        var translation = new ProductTranslation
        {
            ProductId = dto.ProductId,
            LanguageId = language.Id,
            LanguageCode = language.Code,
            Name = dto.Name,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            MetaTitle = dto.MetaTitle,
            MetaDescription = dto.MetaDescription,
            MetaKeywords = dto.MetaKeywords
        };

        await _context.Set<ProductTranslation>().AddAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }

    public async Task<ProductTranslationDto> UpdateProductTranslationAsync(Guid id, CreateProductTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.Name = dto.Name;
        translation.Description = dto.Description;
        translation.ShortDescription = dto.ShortDescription;
        translation.MetaTitle = dto.MetaTitle;
        translation.MetaDescription = dto.MetaDescription;
        translation.MetaKeywords = dto.MetaKeywords;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }

    public async Task<IEnumerable<ProductTranslationDto>> GetProductTranslationsAsync(Guid productId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        var translations = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .Where(pt => pt.ProductId == productId)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<ProductTranslationDto>>(translations);
    }

    public async Task<ProductTranslationDto?> GetProductTranslationAsync(Guid productId, string languageCode)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.ProductId == productId &&
                                      pt.LanguageCode.ToLower() == languageCode.ToLower());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return translation != null ? _mapper.Map<ProductTranslationDto>(translation) : null;
    }

    public async Task DeleteProductTranslationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Category Translations

    public async Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CreateCategoryTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower());

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var exists = await _context.Set<CategoryTranslation>()
            .AnyAsync(ct => ct.CategoryId == dto.CategoryId &&
                           ct.LanguageCode.ToLower() == dto.LanguageCode.ToLower());

        if (exists)
        {
            throw new BusinessException("Bu kategori ve dil için çeviri zaten mevcut.");
        }

        var translation = new CategoryTranslation
        {
            CategoryId = dto.CategoryId,
            LanguageId = language.Id,
            LanguageCode = language.Code,
            Name = dto.Name,
            Description = dto.Description
        };

        await _context.Set<CategoryTranslation>().AddAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }

    public async Task<CategoryTranslationDto> UpdateCategoryTranslationAsync(Guid id, CreateCategoryTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.Name = dto.Name;
        translation.Description = dto.Description;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }

    public async Task<IEnumerable<CategoryTranslationDto>> GetCategoryTranslationsAsync(Guid categoryId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ct.IsDeleted (Global Query Filter)
        var translations = await _context.Set<CategoryTranslation>()
            .AsNoTracking()
            .Where(ct => ct.CategoryId == categoryId)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CategoryTranslationDto>>(translations);
    }

    public async Task<CategoryTranslationDto?> GetCategoryTranslationAsync(Guid categoryId, string languageCode)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.CategoryId == categoryId &&
                                      ct.LanguageCode.ToLower() == languageCode.ToLower());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return translation != null ? _mapper.Map<CategoryTranslationDto>(translation) : null;
    }

    public async Task DeleteCategoryTranslationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Static Translations

    public async Task<StaticTranslationDto> CreateStaticTranslationAsync(CreateStaticTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower());

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var exists = await _context.Set<StaticTranslation>()
            .AnyAsync(st => st.Key == dto.Key &&
                           st.LanguageCode.ToLower() == dto.LanguageCode.ToLower());

        if (exists)
        {
            throw new BusinessException("Bu anahtar ve dil için çeviri zaten mevcut.");
        }

        var translation = new StaticTranslation
        {
            Key = dto.Key,
            LanguageId = language.Id,
            LanguageCode = language.Code,
            Value = dto.Value,
            Category = dto.Category
        };

        await _context.Set<StaticTranslation>().AddAsync(translation);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }

    public async Task<StaticTranslationDto> UpdateStaticTranslationAsync(Guid id, CreateStaticTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.Value = dto.Value;
        translation.Category = dto.Category;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }

    public async Task<Dictionary<string, string>> GetStaticTranslationsAsync(string languageCode, string? category = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var query = _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == languageCode.ToLower());

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(st => st.Category == category);
        }

        var translations = await query.ToDictionaryAsync(st => st.Key, st => st.Value);

        return translations;
    }

    public async Task<string> GetStaticTranslationAsync(string key, string languageCode)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Key == key &&
                                      st.LanguageCode.ToLower() == languageCode.ToLower());

        return translation?.Value ?? key;
    }

    public async Task DeleteStaticTranslationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == id);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task BulkCreateStaticTranslationsAsync(BulkTranslationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower());

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var existingKeys = await _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == dto.LanguageCode.ToLower())
            .Select(st => st.Key)
            .ToListAsync();

        var newTranslations = dto.Translations
            .Where(kvp => !existingKeys.Contains(kvp.Key))
            .Select(kvp => new StaticTranslation
            {
                Key = kvp.Key,
                LanguageId = language.Id,
                LanguageCode = language.Code,
                Value = kvp.Value,
                Category = "UI"
            })
            .ToList();

        await _context.Set<StaticTranslation>().AddRangeAsync(newTranslations);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region User Preferences

    public async Task SetUserLanguagePreferenceAsync(Guid userId, string languageCode)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == languageCode.ToLower() && l.IsActive);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserLanguagePreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference == null)
        {
            preference = new UserLanguagePreference
            {
                UserId = userId,
                LanguageId = language.Id,
                LanguageCode = language.Code
            };
            await _context.Set<UserLanguagePreference>().AddAsync(preference);
        }
        else
        {
            preference.LanguageId = language.Id;
            preference.LanguageCode = language.Code;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> GetUserLanguagePreferenceAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserLanguagePreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference != null)
        {
            return preference.LanguageCode;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsDefault);

        return defaultLanguage?.Code ?? "en";
    }

    #endregion

    #region Statistics

    public async Task<TranslationStatsDto> GetTranslationStatsAsync()
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var totalLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync();

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var activeLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync(l => l.IsActive);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync() ?? "en";

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync();

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var languageCoverage = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .Select(l => new LanguageCoverageDto
            {
                LanguageCode = l.Code,
                LanguageName = l.Name,
                ProductsTranslated = _context.Set<ProductTranslation>()
                    .AsNoTracking()
                    .Count(pt => pt.LanguageCode == l.Code),
                TotalProducts = totalProducts,
                CoveragePercentage = totalProducts > 0
                    ? (decimal)_context.Set<ProductTranslation>()
                        .AsNoTracking()
                        .Count(pt => pt.LanguageCode == l.Code) / totalProducts * 100
                    : 0
            })
            .ToListAsync();

        return new TranslationStatsDto
        {
            TotalLanguages = totalLanguages,
            ActiveLanguages = activeLanguages,
            DefaultLanguage = defaultLanguage,
            LanguageCoverage = languageCoverage
        };
    }

    #endregion

    #region Mapping
    // ✅ ARCHITECTURE: Tüm manuel mapping'ler AutoMapper'a geçirildi
    // Mapping'ler MappingProfile.cs'de tanımlı
    #endregion
}
