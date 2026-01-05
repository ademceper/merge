using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.International;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.International;


namespace Merge.Application.Services.International;

public class LanguageService : ILanguageService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LanguageService> _logger;

    public LanguageService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LanguageService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Language Management

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var languages = await _context.Set<Language>()
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Take(200) // ✅ Güvenlik: Maksimum 200 dil
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<LanguageDto>>(languages);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<LanguageDto>> GetActiveLanguagesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var languages = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Take(100) // ✅ Güvenlik: Maksimum 100 aktif dil
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<LanguageDto>>(languages);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LanguageDto?> GetLanguageByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LanguageDto?> GetLanguageByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == code.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<LanguageDto> CreateLanguageAsync(CreateLanguageDto dto, CancellationToken cancellationToken = default)
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

        _logger.LogInformation("Dil olusturuluyor. Code: {Code}, Name: {Name}", dto.Code, dto.Name);

        try
        {
            // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
            var exists = await _context.Set<Language>()
                .AnyAsync(l => l.Code.ToLower() == dto.Code.ToLower(), cancellationToken);

            if (exists)
            {
                _logger.LogWarning("Dil kodu zaten mevcut. Code: {Code}", dto.Code);
                throw new BusinessException($"Bu dil kodu zaten mevcut: {dto.Code}");
            }

            if (dto.IsDefault)
            {
                // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
                var currentDefault = await _context.Set<Language>()
                    .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

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

            await _context.Set<Language>().AddAsync(language, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Dil olusturuldu. LanguageId: {LanguageId}, Code: {Code}", language.Id, language.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<LanguageDto>(language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dil olusturma hatasi. Code: {Code}, Name: {Name}", dto.Code, dto.Name);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", id);
        }

        language.Name = dto.Name;
        language.NativeName = dto.NativeName;
        language.IsActive = dto.IsActive;
        language.IsRTL = dto.IsRTL;
        language.FlagIcon = dto.FlagIcon;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<LanguageDto>(language);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteLanguageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", id);
        }

        if (language.IsDefault)
        {
            throw new BusinessException("Varsayılan dil silinemez.");
        }

        language.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Product Translations

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductTranslationDto> CreateProductTranslationAsync(CreateProductTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var exists = await _context.Set<ProductTranslation>()
            .AnyAsync(pt => pt.ProductId == dto.ProductId &&
                           pt.LanguageCode.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

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

        await _context.Set<ProductTranslation>().AddAsync(translation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductTranslationDto> UpdateProductTranslationAsync(Guid id, CreateProductTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ProductTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<ProductTranslationDto>> GetProductTranslationsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var translations = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .Where(pt => pt.ProductId == productId)
            .Take(50) // ✅ Güvenlik: Maksimum 50 çeviri
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<ProductTranslationDto>>(translations);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductTranslationDto?> GetProductTranslationAsync(Guid productId, string languageCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.ProductId == productId &&
                                      pt.LanguageCode.ToLower() == languageCode.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return translation != null ? _mapper.Map<ProductTranslationDto>(translation) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteProductTranslationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Category Translations

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CreateCategoryTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var exists = await _context.Set<CategoryTranslation>()
            .AnyAsync(ct => ct.CategoryId == dto.CategoryId &&
                           ct.LanguageCode.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

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

        await _context.Set<CategoryTranslation>().AddAsync(translation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryTranslationDto> UpdateCategoryTranslationAsync(Guid id, CreateCategoryTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.Name = dto.Name;
        translation.Description = dto.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CategoryTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<CategoryTranslationDto>> GetCategoryTranslationsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ct.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var translations = await _context.Set<CategoryTranslation>()
            .AsNoTracking()
            .Where(ct => ct.CategoryId == categoryId)
            .Take(50) // ✅ Güvenlik: Maksimum 50 çeviri
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CategoryTranslationDto>>(translations);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CategoryTranslationDto?> GetCategoryTranslationAsync(Guid categoryId, string languageCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.CategoryId == categoryId &&
                                      ct.LanguageCode.ToLower() == languageCode.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return translation != null ? _mapper.Map<CategoryTranslationDto>(translation) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteCategoryTranslationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted (Global Query Filter)
        var translation = await _context.Set<CategoryTranslation>()
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Static Translations

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StaticTranslationDto> CreateStaticTranslationAsync(CreateStaticTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var exists = await _context.Set<StaticTranslation>()
            .AnyAsync(st => st.Key == dto.Key &&
                           st.LanguageCode.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

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

        await _context.Set<StaticTranslation>().AddAsync(translation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StaticTranslationDto> UpdateStaticTranslationAsync(Guid id, CreateStaticTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == id, cancellationToken);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.Value = dto.Value;
        translation.Category = dto.Category;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<StaticTranslationDto>(translation);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    // ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
    public async Task<Dictionary<string, string>> GetStaticTranslationsAsync(string languageCode, string? category = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var query = _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == languageCode.ToLower());

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(st => st.Category == category);
        }

        // ✅ Güvenlik: Maksimum 1000 çeviri
        var translations = await query
            .Take(1000)
            .ToDictionaryAsync(st => st.Key, st => st.Value, cancellationToken);

        return translations;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> GetStaticTranslationAsync(string key, string languageCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(st => st.Key == key &&
                                      st.LanguageCode.ToLower() == languageCode.ToLower(), cancellationToken);

        return translation?.Value ?? key;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteStaticTranslationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !st.IsDeleted (Global Query Filter)
        var translation = await _context.Set<StaticTranslation>()
            .FirstOrDefaultAsync(st => st.Id == id, cancellationToken);

        if (translation == null)
        {
            throw new NotFoundException("Çeviri", id);
        }

        translation.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task BulkCreateStaticTranslationsAsync(BulkTranslationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == dto.LanguageCode.ToLower(), cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !st.IsDeleted (Global Query Filter)
        var existingKeys = await _context.Set<StaticTranslation>()
            .AsNoTracking()
            .Where(st => st.LanguageCode.ToLower() == dto.LanguageCode.ToLower())
            .Select(st => st.Key)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var newTranslations = new List<StaticTranslation>(dto.Translations.Count);
        foreach (var kvp in dto.Translations)
        {
            if (!existingKeys.Contains(kvp.Key))
            {
                newTranslations.Add(new StaticTranslation
                {
                    Key = kvp.Key,
                    LanguageId = language.Id,
                    LanguageCode = language.Code,
                    Value = kvp.Value,
                    Category = "UI"
                });
            }
        }

        await _context.Set<StaticTranslation>().AddRangeAsync(newTranslations, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region User Preferences

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SetUserLanguagePreferenceAsync(Guid userId, string languageCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == languageCode.ToLower() && l.IsActive, cancellationToken);

        if (language == null)
        {
            throw new NotFoundException("Dil", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserLanguagePreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference == null)
        {
            preference = new UserLanguagePreference
            {
                UserId = userId,
                LanguageId = language.Id,
                LanguageCode = language.Code
            };
            await _context.Set<UserLanguagePreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.LanguageId = language.Id;
            preference.LanguageCode = language.Code;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> GetUserLanguagePreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserLanguagePreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference != null)
        {
            return preference.LanguageCode;
        }

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

        return defaultLanguage?.Code ?? "en";
    }

    #endregion

    #region Statistics

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TranslationStatsDto> GetTranslationStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var totalLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var activeLanguages = await _context.Set<Language>()
            .AsNoTracking()
            .CountAsync(l => l.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !l.IsDeleted (Global Query Filter)
        var defaultLanguage = await _context.Set<Language>()
            .AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "en";

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

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
            .ToListAsync(cancellationToken);

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
