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

public class GetFlashSaleByIdQueryHandler : IRequestHandler<GetFlashSaleByIdQuery, FlashSaleDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetFlashSaleByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FlashSaleDto?> Handle(GetFlashSaleByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - N+1 query önleme (Cartesian Explosion önleme)
        var flashSale = await _context.Set<FlashSale>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(fs => fs.FlashSaleProducts)
                .ThenInclude(fsp => fsp.Product)
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        return flashSale == null ? null : _mapper.Map<FlashSaleDto>(flashSale);
    }
}
