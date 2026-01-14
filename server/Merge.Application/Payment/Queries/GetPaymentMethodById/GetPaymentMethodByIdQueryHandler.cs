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

namespace Merge.Application.Payment.Queries.GetPaymentMethodById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetPaymentMethodByIdQueryHandler : IRequestHandler<GetPaymentMethodByIdQuery, PaymentMethodDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentMethodByIdQueryHandler> _logger;

    public GetPaymentMethodByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPaymentMethodByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentMethodDto?> Handle(GetPaymentMethodByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var paymentMethod = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, cancellationToken);

        if (paymentMethod == null)
        {
            _logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PaymentMethodDto>(paymentMethod);
    }
}
