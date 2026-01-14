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

namespace Merge.Application.International.Queries.GetProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetProductTranslationQueryHandler : IRequestHandler<GetProductTranslationQuery, ProductTranslationDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductTranslationQueryHandler> _logger;

    public GetProductTranslationQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductTranslationQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductTranslationDto?> Handle(GetProductTranslationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product translation. ProductId: {ProductId}, LanguageCode: {LanguageCode}", 
            request.ProductId, request.LanguageCode);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pt.IsDeleted (Global Query Filter)
        var translation = await _context.Set<ProductTranslation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.ProductId == request.ProductId &&
                                      pt.LanguageCode.ToLower() == request.LanguageCode.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return translation != null ? _mapper.Map<ProductTranslationDto>(translation) : null;
    }
}

