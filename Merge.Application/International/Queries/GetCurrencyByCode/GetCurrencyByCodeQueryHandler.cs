using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetCurrencyByCodeQueryHandler : IRequestHandler<GetCurrencyByCodeQuery, CurrencyDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCurrencyByCodeQueryHandler> _logger;

    public GetCurrencyByCodeQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCurrencyByCodeQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto?> Handle(GetCurrencyByCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting currency by code. Code: {Code}", request.Code);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var currency = await _context.Set<Currency>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return currency != null ? _mapper.Map<CurrencyDto>(currency) : null;
    }
}

