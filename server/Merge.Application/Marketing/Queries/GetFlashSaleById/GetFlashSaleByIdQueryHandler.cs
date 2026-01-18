using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetFlashSaleById;

public class GetFlashSaleByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetFlashSaleByIdQuery, FlashSaleDto?>
{
    public async Task<FlashSaleDto?> Handle(GetFlashSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var flashSale = await context.Set<FlashSale>()
            .AsNoTracking()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        return flashSale == null ? null : mapper.Map<FlashSaleDto>(flashSale);
    }
}
