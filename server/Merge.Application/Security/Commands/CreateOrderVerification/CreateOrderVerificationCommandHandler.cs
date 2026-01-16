using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using OrderItemEntity = Merge.Domain.Modules.Ordering.OrderItem;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.CreateOrderVerification;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateOrderVerificationCommandHandler : IRequestHandler<CreateOrderVerificationCommand, OrderVerificationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderVerificationCommandHandler> _logger;
    private readonly SecuritySettings _securitySettings;

    public CreateOrderVerificationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateOrderVerificationCommandHandler> logger,
        IOptions<SecuritySettings> securitySettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _securitySettings = securitySettings.Value;
    }

    public async Task<OrderVerificationDto> Handle(CreateOrderVerificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order verification oluşturuluyor. OrderId: {OrderId}, VerificationType: {VerificationType}",
            request.OrderId, request.VerificationType);

        var order = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // Check if verification already exists
        var existing = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.OrderId == request.OrderId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Bu sipariş için zaten bir doğrulama kaydı var.");
        }

        // Calculate risk score
        var riskScore = await CalculateOrderRiskScoreAsync(request.OrderId, cancellationToken);

        // Parse enum
        var verificationType = Enum.TryParse<VerificationType>(request.VerificationType, true, out var parsedType)
            ? parsedType
            : throw new BusinessException($"Invalid VerificationType: {request.VerificationType}");

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var verification = OrderVerification.Create(
            orderId: request.OrderId,
            verificationType: verificationType,
            riskScore: riskScore,
            verificationMethod: request.VerificationMethod,
            verificationNotes: request.VerificationNotes,
            requiresManualReview: request.RequiresManualReview || riskScore >= _securitySettings.OrderVerificationManualReviewThreshold);

        await _context.Set<OrderVerification>().AddAsync(verification, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        verification = await _context.Set<OrderVerification>()
            .AsNoTracking()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .FirstOrDefaultAsync(v => v.Id == verification.Id, cancellationToken);

        _logger.LogInformation("Order verification oluşturuldu. VerificationId: {VerificationId}, OrderId: {OrderId}, RiskScore: {RiskScore}",
            verification!.Id, request.OrderId, riskScore);

        return _mapper.Map<OrderVerificationDto>(verification);
    }

    private async Task<int> CalculateOrderRiskScoreAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return 0;

        int riskScore = 0;

        // High value order - ✅ BOLUM 12.0: Magic number config'den
        if (order.TotalAmount > _securitySettings.HighValueOrderThreshold) 
            riskScore += _securitySettings.HighValueOrderRiskWeight;

        // New user - ✅ BOLUM 12.0: Magic number config'den
        var daysSinceRegistration = (DateTime.UtcNow - order.User.CreatedAt).Days;
        if (daysSinceRegistration < _securitySettings.NewUserRiskDays) 
            riskScore += _securitySettings.NewUserRiskWeight;

        // Multiple items - ✅ BOLUM 12.0: Magic number config'den
        var itemCount = await _context.Set<OrderItem>()
            .AsNoTracking()
            .CountAsync(oi => oi.OrderId == orderId, cancellationToken);
        if (itemCount > _securitySettings.MultipleItemsThreshold) 
            riskScore += _securitySettings.MultipleItemsRiskWeight;

        // High quantity - ✅ BOLUM 12.0: Magic number config'den
        var totalQuantity = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.OrderId == orderId)
            .SumAsync(oi => oi.Quantity, cancellationToken);
        if (totalQuantity > _securitySettings.HighQuantityThreshold) 
            riskScore += _securitySettings.HighQuantityRiskWeight;

        return Math.Min(riskScore, _securitySettings.MaxRiskScore);
    }
}
