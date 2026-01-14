using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPurchaseOrderByPONumberQueryHandler : IRequestHandler<GetPurchaseOrderByPONumberQuery, PurchaseOrderDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPurchaseOrderByPONumberQueryHandler> _logger;

    public GetPurchaseOrderByPONumberQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPurchaseOrderByPONumberQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PurchaseOrderDto?> Handle(GetPurchaseOrderByPONumberQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple collection includes)
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.ApprovedBy)
            .Include(po => po.CreditTerm)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.PONumber == request.PONumber, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return po != null ? _mapper.Map<PurchaseOrderDto>(po) : null;
    }
}

