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

namespace Merge.Application.International.Queries.GetLanguageByCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetLanguageByCodeQueryHandler : IRequestHandler<GetLanguageByCodeQuery, LanguageDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLanguageByCodeQueryHandler> _logger;

    public GetLanguageByCodeQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetLanguageByCodeQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LanguageDto?> Handle(GetLanguageByCodeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting language by code. Code: {Code}", request.Code);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code.ToLower() == request.Code.ToLower(), cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }
}

