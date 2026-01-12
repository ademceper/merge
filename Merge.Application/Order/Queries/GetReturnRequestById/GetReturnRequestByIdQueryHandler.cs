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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReturnRequestByIdQueryHandler : IRequestHandler<GetReturnRequestByIdQuery, ReturnRequestDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetReturnRequestByIdQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReturnRequestDto?> Handle(GetReturnRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var returnRequest = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.Order)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnRequestId, cancellationToken);

        return returnRequest != null ? _mapper.Map<ReturnRequestDto>(returnRequest) : null;
    }
}
