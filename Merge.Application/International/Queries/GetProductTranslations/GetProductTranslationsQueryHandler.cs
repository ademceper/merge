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

namespace Merge.Application.International.Queries.GetProductTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetProductTranslationsQueryHandler : IRequestHandler<GetProductTranslationsQuery, IEnumerable<ProductTranslationDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductTranslationsQueryHandler> _logger;

    public GetProductTranslationsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductTranslationsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductTranslationDto>> Handle(GetProductTranslationsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product translations. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var translations = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .Where(pt => pt.ProductId == request.ProductId)
            .Take(50) // ✅ Güvenlik: Maksimum 50 çeviri
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<ProductTranslationDto>>(translations);
    }
}

