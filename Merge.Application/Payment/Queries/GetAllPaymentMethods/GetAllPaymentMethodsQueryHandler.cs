using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Payment.Queries.GetAllPaymentMethods;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetAllPaymentMethodsQueryHandler : IRequestHandler<GetAllPaymentMethodsQuery, IEnumerable<PaymentMethodDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllPaymentMethodsQueryHandler> _logger;

    public GetAllPaymentMethodsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllPaymentMethodsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentMethodDto>> Handle(GetAllPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all payment methods. IsActive: {IsActive}", request.IsActive);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var query = _context.Set<PaymentMethod>()
            .AsNoTracking()
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(pm => pm.IsActive == request.IsActive.Value);
        }

        var methods = await query
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select(MapToDto) YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }
}
