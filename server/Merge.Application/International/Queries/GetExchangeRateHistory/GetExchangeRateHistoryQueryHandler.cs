using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetExchangeRateHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetExchangeRateHistoryQueryHandler : IRequestHandler<GetExchangeRateHistoryQuery, IEnumerable<ExchangeRateHistoryDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetExchangeRateHistoryQueryHandler> _logger;

    public GetExchangeRateHistoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetExchangeRateHistoryQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ExchangeRateHistoryDto>> Handle(GetExchangeRateHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting exchange rate history. CurrencyCode: {CurrencyCode}, Days: {Days}", 
            request.CurrencyCode, request.Days);

        var startDate = DateTime.UtcNow.AddDays(-request.Days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !h.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var history = await _context.Set<ExchangeRateHistory>()
            .AsNoTracking()
            .Where(h => h.CurrencyCode.ToUpper() == request.CurrencyCode.ToUpper() &&
                       h.RecordedAt >= startDate)
            .OrderByDescending(h => h.RecordedAt)
            .Take(1000) // ✅ Güvenlik: Maksimum 1000 kayıt
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<ExchangeRateHistoryDto>>(history);
    }
}

