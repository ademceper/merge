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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetVerificationByOrderIdQueryHandler : IRequestHandler<GetVerificationByOrderIdQuery, OrderVerificationDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVerificationByOrderIdQueryHandler> _logger;

    public GetVerificationByOrderIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetVerificationByOrderIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderVerificationDto?> Handle(GetVerificationByOrderIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Order verification sorgulanıyor. OrderId: {OrderId}", request.OrderId);

        var verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.OrderId == request.OrderId, cancellationToken);

        if (verification == null)
        {
            _logger.LogWarning("Order verification bulunamadı. OrderId: {OrderId}", request.OrderId);
            return null;
        }

        _logger.LogInformation("Order verification bulundu. VerificationId: {VerificationId}, OrderId: {OrderId}",
            verification.Id, request.OrderId);

        return _mapper.Map<OrderVerificationDto>(verification);
    }
}
