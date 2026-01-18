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

public class GetCreditTermByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCreditTermByIdQueryHandler> logger) : IRequestHandler<GetCreditTermByIdQuery, CreditTermDto?>
{

    public async Task<CreditTermDto?> Handle(GetCreditTermByIdQuery request, CancellationToken cancellationToken)
    {
        var creditTerm = await context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == request.Id, cancellationToken);

        return creditTerm is not null ? mapper.Map<CreditTermDto>(creditTerm) : null;
    }
}

