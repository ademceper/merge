using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Payment.Queries.GetAvailablePaymentMethods;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetAvailablePaymentMethodsQueryHandler : IRequestHandler<GetAvailablePaymentMethodsQuery, IEnumerable<PaymentMethodDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAvailablePaymentMethodsQueryHandler> _logger;

    public GetAvailablePaymentMethodsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAvailablePaymentMethodsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentMethodDto>> Handle(GetAvailablePaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving available payment methods. OrderAmount: {OrderAmount}", request.OrderAmount);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan (IsAmountValid)
        var methods = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .Where(pm => pm.IsActive &&
                  (!pm.MinimumAmount.HasValue || request.OrderAmount >= pm.MinimumAmount.Value) &&
                  (!pm.MaximumAmount.HasValue || request.OrderAmount <= pm.MaximumAmount.Value))
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select(MapToDto) YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }
}
