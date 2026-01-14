using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetActiveLanguages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetActiveLanguagesQueryHandler : IRequestHandler<GetActiveLanguagesQuery, IEnumerable<LanguageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveLanguagesQueryHandler> _logger;

    public GetActiveLanguagesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActiveLanguagesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<LanguageDto>> Handle(GetActiveLanguagesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active languages");

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
}

