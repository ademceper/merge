using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Cart.Queries.GetPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderQueryHandler : IRequestHandler<GetPreOrderQuery, PreOrderDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetPreOrderQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PreOrderDto?> Handle(GetPreOrderQuery request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

        return preOrder != null ? _mapper.Map<PreOrderDto>(preOrder) : null;
    }
}

