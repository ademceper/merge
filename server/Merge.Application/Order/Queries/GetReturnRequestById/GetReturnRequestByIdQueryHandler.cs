using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetReturnRequestById;

public class GetReturnRequestByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetReturnRequestByIdQuery, ReturnRequestDto?>
{

    public async Task<ReturnRequestDto?> Handle(GetReturnRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var returnRequest = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        return returnRequest is not null ? mapper.Map<ReturnRequestDto>(returnRequest) : null;
    }
}
