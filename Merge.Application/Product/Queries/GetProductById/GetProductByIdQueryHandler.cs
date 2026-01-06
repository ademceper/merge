using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Product.Queries.GetProductById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // PERFORMANCE: AsNoTracking for read-only queries
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        return product == null ? null : _mapper.Map<ProductDto>(product);
    }
}
