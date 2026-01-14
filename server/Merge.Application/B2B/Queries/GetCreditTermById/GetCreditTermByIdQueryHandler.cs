using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetCreditTermById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCreditTermByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCreditTermByIdQueryHandler> logger) : IRequestHandler<GetCreditTermByIdQuery, CreditTermDto?>
{

    public async Task<CreditTermDto?> Handle(GetCreditTermByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted check (Global Query Filter handles it)
        var creditTerm = await context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return creditTerm != null ? mapper.Map<CreditTermDto>(creditTerm) : null;
    }
}

