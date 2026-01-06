using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.B2B.Queries.GetCreditTermById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCreditTermByIdQueryHandler : IRequestHandler<GetCreditTermByIdQuery, CreditTermDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCreditTermByIdQueryHandler> _logger;

    public GetCreditTermByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCreditTermByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreditTermDto?> Handle(GetCreditTermByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return creditTerm != null ? _mapper.Map<CreditTermDto>(creditTerm) : null;
    }
}

