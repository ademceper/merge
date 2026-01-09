using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.International.Queries.GetLanguageById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetLanguageByIdQueryHandler : IRequestHandler<GetLanguageByIdQuery, LanguageDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLanguageByIdQueryHandler> _logger;

    public GetLanguageByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetLanguageByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LanguageDto?> Handle(GetLanguageByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting language by ID. LanguageId: {LanguageId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !l.IsDeleted (Global Query Filter)
        var language = await _context.Set<Language>()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return language != null ? _mapper.Map<LanguageDto>(language) : null;
    }
}

