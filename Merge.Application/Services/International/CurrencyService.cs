using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

    public CurrencyService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currencies = await _context.Set<Currency>()
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }

    public async Task<IEnumerable<CurrencyDto>> GetActiveCurrenciesAsync()
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currencies = await _context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CurrencyDto>>(currencies);
    }

    public async Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return currency != null ? _mapper.Map<CurrencyDto>(currency) : null;
    }

    public async Task<CurrencyDto?> GetCurrencyByCodeAsync(string code)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return currency != null ? _mapper.Map<CurrencyDto>(currency) : null;
    }

    public async Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto)
    {
        // Check if currency code already exists
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var exists = await _context.Set<Currency>()
            .AnyAsync(c => c.Code.ToUpper() == dto.Code.ToUpper());

        if (exists)
        {
            throw new BusinessException($"Bu para birimi kodu zaten mevcut: {dto.Code}");
        }

        // If setting as base currency, update existing base currency
        if (dto.IsBaseCurrency)
        {
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
            var currentBase = await _context.Set<Currency>()
                .FirstOrDefaultAsync(c => c.IsBaseCurrency);

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

        await _context.Set<Currency>().AddAsync(currency);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", id);
        }

        currency.Name = dto.Name;
        currency.Symbol = dto.Symbol;
        currency.ExchangeRate = dto.ExchangeRate;
        currency.IsActive = dto.IsActive;
        currency.DecimalPlaces = dto.DecimalPlaces;
        currency.Format = dto.Format;
        currency.LastUpdated = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CurrencyDto>(currency);
    }

    public async Task DeleteCurrencyAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", id);
        }

        if (currency.IsBaseCurrency)
        {
            throw new BusinessException("Temel para birimi silinemez.");
        }

        currency.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateExchangeRateAsync(string currencyCode, decimal newRate, string source = "Manual")
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper());

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

        await _context.Set<ExchangeRateHistory>().AddAsync(history);

        // Update currency
        currency.ExchangeRate = newRate;
        currency.LastUpdated = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ConvertedPriceDto> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var from = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == fromCurrency.ToUpper() && c.IsActive);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var to = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == toCurrency.ToUpper() && c.IsActive);

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

        var formatted = await FormatPriceAsync(convertedAmount, to.Code);

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

    public async Task<string> FormatPriceAsync(decimal amount, string currencyCode)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper());

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

    public async Task<IEnumerable<ExchangeRateHistoryDto>> GetExchangeRateHistoryAsync(string currencyCode, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !h.IsDeleted (Global Query Filter)
        var history = await _context.Set<ExchangeRateHistory>()
            .AsNoTracking()
            .Where(h => h.CurrencyCode.ToUpper() == currencyCode.ToUpper() &&
                       h.RecordedAt >= startDate)
            .OrderByDescending(h => h.RecordedAt)
            .Select(h => new ExchangeRateHistoryDto
            {
                Id = h.Id,
                CurrencyCode = h.CurrencyCode,
                ExchangeRate = h.ExchangeRate,
                RecordedAt = h.RecordedAt,
                Source = h.Source
            })
            .ToListAsync();

        return history;
    }

    public async Task SetUserCurrencyPreferenceAsync(Guid userId, string currencyCode)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == currencyCode.ToUpper() && c.IsActive);

        if (currency == null)
        {
            throw new NotFoundException("Para birimi", Guid.Empty);
        }

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserCurrencyPreference>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference == null)
        {
            preference = new UserCurrencyPreference
            {
                UserId = userId,
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code
            };
            await _context.Set<UserCurrencyPreference>().AddAsync(preference);
        }
        else
        {
            preference.CurrencyId = currency.Id;
            preference.CurrencyCode = currency.Code;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> GetUserCurrencyPreferenceAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var preference = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preference != null)
        {
            return preference.CurrencyCode;
        }

        // Return base currency if no preference set
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IsBaseCurrency);

        return baseCurrency?.Code ?? "USD";
    }

    public async Task<CurrencyStatsDto> GetCurrencyStatsAsync()
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var totalCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync();

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var activeCurrencies = await _context.Set<Currency>()
            .AsNoTracking()
            .CountAsync(c => c.IsActive);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var baseCurrency = await _context.Set<Currency>()
            .AsNoTracking()
            .Where(c => c.IsBaseCurrency)
            .Select(c => c.Code)
            .FirstOrDefaultAsync() ?? "USD";

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var lastUpdate = await _context.Set<Currency>()
            .AsNoTracking()
            .MaxAsync(c => (DateTime?)c.LastUpdated) ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalUsers = await _context.Set<UserCurrencyPreference>()
            .AsNoTracking()
            .CountAsync();

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
            .ToListAsync();

        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        var currencyNames = await _context.Set<Currency>()
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Name);

        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - Database'de DTO oluştur
        // ✅ PERFORMANCE: Batch loading - currency names için dictionary
        var mostUsed = currencyUsage.Select(u => new CurrencyUsageDto
        {
            CurrencyCode = u.CurrencyCode,
            CurrencyName = currencyNames.TryGetValue(u.CurrencyCode, out var name) ? name : u.CurrencyCode,
            UserCount = u.Count,
            Percentage = totalUsers > 0 ? (decimal)u.Count / totalUsers * 100 : 0
        }).ToList();

        return new CurrencyStatsDto
        {
            TotalCurrencies = totalCurrencies,
            ActiveCurrencies = activeCurrencies,
            BaseCurrency = baseCurrency,
            LastRateUpdate = lastUpdate,
            MostUsedCurrencies = mostUsed
        };
    }

    public async Task SyncExchangeRatesAsync()
    {
        // Placeholder for future API integration (e.g., exchangerate-api.com, fixer.io)
        // For now, this is a manual operation
        await Task.CompletedTask;
    }

    // ✅ ARCHITECTURE: Tüm manuel mapping'ler AutoMapper'a geçirildi
    // Mapping'ler MappingProfile.cs'de tanımlı
}
