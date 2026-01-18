using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerApplication;

public class GetSellerApplicationQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerApplicationQueryHandler> logger) : IRequestHandler<GetSellerApplicationQuery, SellerApplicationDto?>
{

    public async Task<SellerApplicationDto?> Handle(GetSellerApplicationQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller application. ApplicationId: {ApplicationId}", request.ApplicationId);

        var application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        return application is null ? null : mapper.Map<SellerApplicationDto>(application);
    }
}
