using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetVerificationByOrderId;

public class GetVerificationByOrderIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetVerificationByOrderIdQueryHandler> logger) : IRequestHandler<GetVerificationByOrderIdQuery, OrderVerificationDto?>
{

    public async Task<OrderVerificationDto?> Handle(GetVerificationByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order verification sorgulanıyor. OrderId: {OrderId}", request.OrderId);

        var verification = await context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.OrderId == request.OrderId, cancellationToken);

        if (verification is null)
        {
            logger.LogWarning("Order verification bulunamadı. OrderId: {OrderId}", request.OrderId);
            return null;
        }

        logger.LogInformation("Order verification bulundu. VerificationId: {VerificationId}, OrderId: {OrderId}",
            verification.Id, request.OrderId);

        return mapper.Map<OrderVerificationDto>(verification);
    }
}
