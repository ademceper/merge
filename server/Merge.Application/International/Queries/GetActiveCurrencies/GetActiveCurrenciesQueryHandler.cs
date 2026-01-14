using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetActiveCurrencies;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetActiveCurrenciesQueryHandler : IRequestHandler<GetActiveCurrenciesQuery, IEnumerable<CurrencyDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveCurrenciesQueryHandler> _logger;

    public GetActiveCurrenciesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActiveCurrenciesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CurrencyDto>> Handle(GetActiveCurrenciesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active currencies");

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
}

