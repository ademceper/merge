using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Queries.GetCategoryTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetCategoryTranslationsQueryHandler : IRequestHandler<GetCategoryTranslationsQuery, IEnumerable<CategoryTranslationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoryTranslationsQueryHandler> _logger;

    public GetCategoryTranslationsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCategoryTranslationsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryTranslationDto>> Handle(GetCategoryTranslationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting category translations. CategoryId: {CategoryId}", request.CategoryId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ct.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var translations = await _context.Set<CategoryTranslation>()
            .AsNoTracking()
            .Where(ct => ct.CategoryId == request.CategoryId)
            .Take(50) // ✅ Güvenlik: Maksimum 50 çeviri
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<CategoryTranslationDto>>(translations);
    }
}

