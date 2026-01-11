using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using PaymentEntity = Merge.Domain.Entities.Payment;

namespace Merge.Application.Payment.Queries.GetPaymentById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPaymentByIdQueryHandler> _logger;

    public GetPaymentByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetPaymentByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment with ID: {PaymentId}", request.PaymentId);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var payment = await _context.Set<PaymentEntity>()
            .AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found with ID: {PaymentId}", request.PaymentId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PaymentDto>(payment);
    }
}
