using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetInvoice;

public class GetInvoiceQueryHandler(IDbContext context, IMapper mapper, ILogger<GetInvoiceQueryHandler> logger) : IRequestHandler<GetInvoiceQuery, SellerInvoiceDto?>
{

    public async Task<SellerInvoiceDto?> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await context.Set<SellerInvoice>()
            .AsNoTracking()
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        return invoice != null ? mapper.Map<SellerInvoiceDto>(invoice) : null;
    }
}
