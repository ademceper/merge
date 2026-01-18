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

        return preOrder is not null ? mapper.Map<PreOrderDto>(preOrder) : null;
    }
}

