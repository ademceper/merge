using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetCheckByPaymentId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCheckByPaymentIdQueryHandler : IRequestHandler<GetCheckByPaymentIdQuery, PaymentFraudPreventionDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCheckByPaymentIdQueryHandler> _logger;

    public GetCheckByPaymentIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCheckByPaymentIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentFraudPreventionDto?> Handle(GetCheckByPaymentIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Payment fraud check sorgulanıyor. PaymentId: {PaymentId}", request.PaymentId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var check = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.PaymentId == request.PaymentId, cancellationToken);

        if (check == null)
        {
            _logger.LogWarning("Payment fraud check bulunamadı. PaymentId: {PaymentId}", request.PaymentId);
            return null;
        }

        _logger.LogInformation("Payment fraud check bulundu. CheckId: {CheckId}, PaymentId: {PaymentId}",
            check.Id, request.PaymentId);

        return _mapper.Map<PaymentFraudPreventionDto>(check);
    }
}
