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

namespace Merge.Application.Seller.Queries.GetUserSellerApplication;

public class GetUserSellerApplicationQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserSellerApplicationQueryHandler> logger) : IRequestHandler<GetUserSellerApplicationQuery, SellerApplicationDto?>
{

    public async Task<SellerApplicationDto?> Handle(GetUserSellerApplicationQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting user seller application. UserId: {UserId}", request.UserId);

        var application = await context.Set<SellerApplication>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Reviewer)
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return application == null ? null : mapper.Map<SellerApplicationDto>(application);
    }
}
