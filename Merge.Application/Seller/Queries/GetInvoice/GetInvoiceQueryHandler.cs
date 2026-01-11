using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Seller.Queries.GetInvoice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, SellerInvoiceDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetInvoiceQueryHandler> _logger;

    public GetInvoiceQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetInvoiceQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SellerInvoiceDto?> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return invoice != null ? _mapper.Map<SellerInvoiceDto>(invoice) : null;
    }
}
