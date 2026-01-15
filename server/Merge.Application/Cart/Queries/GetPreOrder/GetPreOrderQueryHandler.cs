using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetPreOrderQuery, PreOrderDto?>
{

    public async Task<PreOrderDto?> Handle(GetPreOrderQuery request, CancellationToken cancellationToken)
    {
        var preOrder = await context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        return preOrder is not null ? mapper.Map<PreOrderDto>(preOrder) : null;
    }
}

