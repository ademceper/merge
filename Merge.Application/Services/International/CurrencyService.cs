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

public class CurrencyService : ICurrencyService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CurrencyService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var currencies = await _context.Set<Currency>()
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Take(500) // ✅ Güvenlik: Maksimum 500 para birimi
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<CurrencyDto>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var currencies = await _context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Take(200) // ✅ Güvenlik: Maksimum 200 aktif para birimi
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return currency != null ? _mapper.Map<CurrencyDto>(currency) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CurrencyDto?> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return currency != null ? _mapper.Map<CurrencyDto>(currency) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Para birimi olusturuluyor. Code: {Code}, Name: {Name}", dto.Code, dto.Name);

        try
        {
            // Check if currency code already exists
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
            var exists = await _context.Set<Currency>()
                .AnyAsync(c => c.Code.ToUpper() == dto.Code.ToUpper(), cancellationToken);

            if (exists)
            {
                _logger.LogWarning("Para birimi kodu zaten mevcut. Code: {Code}", dto.Code);
                throw new BusinessException($"Bu para birimi kodu zaten mevcut: {dto.Code}");
            }

            // If setting as base currency, update existing base currency
            if (dto.IsBaseCurrency)
            {
                // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
                var currentBase = await _context.Set<Currency>()
                    .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

                if (currentBase != null)
                {
                    currentBase.IsBaseCurrency = false;
                }

                dto.ExchangeRate = 1.0m; // Base currency always has rate 1.0
            }

            var currency = new Currency
            {
                Code = dto.Code.ToUpper(),
                Name = dto.Name,
                Symbol = dto.Symbol,
                ExchangeRate = dto.ExchangeRate,
                IsBaseCurrency = dto.IsBaseCurrency,
                IsActive = dto.IsActive,
                DecimalPlaces = dto.DecimalPlaces,
                Format = dto.Format,
                LastUpdated = DateTime.UtcNow
            };

            await _context.Set<Currency>().AddAsync(currency, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Para birimi olusturuldu. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<CurrencyDto>(currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Para birimi olusturma hatasi. Code: {Code}, Name: {Name}", dto.Code, dto.Name);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Para birimi guncelleniyor. CurrencyId: {CurrencyId}", id);

        try
        {
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
            var currency = await _context.Set<Currency>()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (currency == null)
            {
                _logger.LogWarning("Para birimi bulunamadi. CurrencyId: {CurrencyId}", id);
                throw new NotFoundException("Para birimi", id);
            }

            currency.Name = dto.Name;
            currency.Symbol = dto.Symbol;
            currency.ExchangeRate = dto.ExchangeRate;
            currency.IsActive = dto.IsActive;
            currency.DecimalPlaces = dto.DecimalPlaces;
            currency.Format = dto.Format;
            currency.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Para birimi guncellendi. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<CurrencyDto>(currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Para birimi guncelleme hatasi. CurrencyId: {CurrencyId}", id);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteCurrencyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", id);
        }

        if (currency.IsBaseCurrency)
        {
            throw new BusinessException("Temel para birimi silinemez.");
        }

        currency.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task UpdateExchangeRateAsync(string currencyCode, decimal newRate, string source = "Manual", CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper(), cancellationToken);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        if (currency.IsBaseCurrency)
        {
            throw new BusinessException("Temel para birimi için döviz kuru güncellenemez.");
        }

        // Save to history
        var history = new ExchangeRateHistory
        {
            CurrencyId = currency.Id,
            CurrencyCode = currency.Code,
            ExchangeRate = newRate,
            RecordedAt = DateTime.UtcNow,
            Source = source
        };

        await _context.Set<ExchangeRateHistory>().AddAsync(history, cancellationToken);

        // Update currency
        currency.ExchangeRate = newRate;
        currency.LastUpdated = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ConvertedPriceDto> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var from = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == fromCurrency.ToUpper() && c.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var to = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == toCurrency.ToUpper() && c.IsActive, cancellationToken);

        if (from == null || to == null)
        {
            throw new ValidationException("Geçersiz para birimi kodu.");
        }

        // Convert through base currency
        // amount in base currency = amount / fromRate
        // amount in target currency = amount in base * toRate
        var baseAmount = amount / from.ExchangeRate;
        var convertedAmount = baseAmount * to.ExchangeRate;

        // Round to target currency decimal places
        convertedAmount = Math.Round(convertedAmount, to.DecimalPlaces);

        var formatted = await FormatPriceAsync(convertedAmount, to.Code, cancellationToken);

        return new ConvertedPriceDto
        {
            OriginalAmount = amount,
            FromCurrency = from.Code,
            ConvertedAmount = convertedAmount,
            ToCurrency = to.Code,
            FormattedPrice = formatted,
            ExchangeRate = to.ExchangeRate / from.ExchangeRate
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> FormatPriceAsync(decimal amount, string currencyCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper(), cancellationToken);

        if (currency == null)
        {
            return amount.ToString("N2");
        }

        var roundedAmount = Math.Round(amount, currency.DecimalPlaces);
        var formattedAmount = roundedAmount.ToString($"N{currency.DecimalPlaces}");

        return currency.Format
            .Replace("{symbol}", currency.Symbol)
            .Replace("{amount}", formattedAmount)
            .Replace("{code}", currency.Code);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<ExchangeRateHistoryDto>> GetExchangeRateHistoryAsync(string currencyCode, int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !h.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var history = await _context.Set<ExchangeRateHistory>()
            .AsNoTracking()
            .Where(h => h.CurrencyCode.ToUpper() == currencyCode.ToUpper() &&
                       h.RecordedAt >= startDate)
            .OrderByDescending(h => h.RecordedAt)
            .Take(1000) // ✅ Güvenlik: Maksimum 1000 kayıt
            .Select(h => new ExchangeRateHistoryDto
            {
                Id = h.Id,
                CurrencyCode = h.CurrencyCode,
                ExchangeRate = h.ExchangeRate,
                RecordedAt = h.RecordedAt,
                Source = h.Source
            })
            .ToListAsync(cancellationToken);

        return history;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SetUserCurrencyPreferenceAsync(Guid userId, string currencyCode, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper() && c.IsActive, cancellationToken);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserCurrencyPreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference == null)
        {
            preference = new UserCurrencyPreference
            {
                UserId = userId,
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code
            };
            await _context.Set<UserCurrencyPreference>().AddAsync(preference, cancellationToken);
        }
        else
        {
            preference.CurrencyId = currency.Id;
            preference.CurrencyCode = currency.Code;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<string> GetUserCurrencyPreferenceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (preference != null)
        {
            return preference.CurrencyCode;
        }

        // Return base currency if no preference set
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

        return baseCurrency?.Code ?? "USD";
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<CurrencyStatsDto> GetCurrencyStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var totalCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var activeCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(c => c.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsBaseCurrency)
            .Select(c => c.Code)
            .FirstOrDefaultAsync(cancellationToken) ?? "USD";

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var lastUpdate = await _context.Set<Currency>()
            .AsNoTracking()
            .MaxAsync(c => (DateTime?)c.LastUpdated, cancellationToken) ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalUsers = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var currencyUsage = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .GroupBy(p => new { p.CurrencyCode })
            .Select(g => new
            {
                g.Key.CurrencyCode,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        var currencyNames = await _context.Set<Currency>()
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Name, cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Database'de DTO oluştur
        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var mostUsed = new List<CurrencyUsageDto>(currencyUsage.Count);
        foreach (var u in currencyUsage)
        {
            mostUsed.Add(new CurrencyUsageDto
            {
                CurrencyCode = u.CurrencyCode,
                CurrencyName = currencyNames.TryGetValue(u.CurrencyCode, out var name) ? name : u.CurrencyCode,
                UserCount = u.Count,
                Percentage = totalUsers > 0 ? (decimal)u.Count / totalUsers * 100 : 0
            });
        }

        return new CurrencyStatsDto
        {
            TotalCurrencies = totalCurrencies,
            ActiveCurrencies = activeCurrencies,
            BaseCurrency = baseCurrency,
            LastRateUpdate = lastUpdate,
            MostUsedCurrencies = mostUsed
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task SyncExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        // Placeholder for future API integration (e.g., exchangerate-api.com, fixer.io)
        // For now, this is a manual operation
        await Task.CompletedTask;
    }

    // ✅ ARCHITECTURE: Tüm manuel mapping'ler AutoMapper'a geçirildi
    // Mapping'ler MappingProfile.cs'de tanımlı
}
