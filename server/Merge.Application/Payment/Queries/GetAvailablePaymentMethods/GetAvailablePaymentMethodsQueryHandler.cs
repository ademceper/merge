using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.GetAvailablePaymentMethods;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetAvailablePaymentMethodsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAvailablePaymentMethodsQueryHandler> logger) : IRequestHandler<GetAvailablePaymentMethodsQuery, IEnumerable<PaymentMethodDto>>
{

    public async Task<IEnumerable<PaymentMethodDto>> Handle(GetAvailablePaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving available payment methods. OrderAmount: {OrderAmount}", request.OrderAmount);

        var methods = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .Where(pm => pm.IsActive &&
                  (!pm.MinimumAmount.HasValue || request.OrderAmount >= pm.MinimumAmount.Value) &&
                  (!pm.MaximumAmount.HasValue || request.OrderAmount <= pm.MaximumAmount.Value))
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }
}
