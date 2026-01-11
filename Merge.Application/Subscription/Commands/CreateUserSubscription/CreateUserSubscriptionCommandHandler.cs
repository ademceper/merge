using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Commands.CreateUserSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateUserSubscriptionCommandHandler : IRequestHandler<CreateUserSubscriptionCommand, UserSubscriptionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateUserSubscriptionCommandHandler> _logger;

    public CreateUserSubscriptionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateUserSubscriptionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserSubscriptionDto> Handle(CreateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating user subscription. UserId: {UserId}, PlanId: {PlanId}",
            request.UserId, request.SubscriptionPlanId);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == request.SubscriptionPlanId && p.IsActive, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", request.SubscriptionPlanId);
        }

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // Check if user already has an active subscription
        var existingActive = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .FirstOrDefaultAsync(us => us.UserId == request.UserId && 
                                    (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial), 
                                    cancellationToken);

        if (existingActive != null)
        {
            throw new BusinessException("Kullanıcının zaten aktif bir aboneliği var.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var subscription = UserSubscription.Create(
            userId: request.UserId,
            plan: plan,
            autoRenew: request.AutoRenew,
            paymentMethodId: request.PaymentMethodId);

        await _context.Set<UserSubscription>().AddAsync(subscription, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create initial payment if not trial
        if (subscription.Status != SubscriptionStatus.Trial)
        {
            var billingPeriodStart = subscription.NextBillingDate ?? subscription.StartDate;
            var billingPeriodEnd = billingPeriodStart.AddDays(plan.DurationDays);
            
            var payment = SubscriptionPayment.Create(
                subscription: subscription,
                amount: plan.Price,
                billingPeriodStart: billingPeriodStart,
                billingPeriodEnd: billingPeriodEnd);

            await _context.Set<SubscriptionPayment>().AddAsync(payment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ✅ PERFORMANCE: Reload with includes for mapping
        subscription = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == subscription.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("User subscription created successfully. SubscriptionId: {SubscriptionId}, UserId: {UserId}",
            subscription!.Id, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<UserSubscriptionDto>(subscription);
        dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            : 0;

        // ✅ PERFORMANCE: Batch load recent payments
        var recentPayments = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == subscription.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        dto.RecentPayments = _mapper.Map<List<SubscriptionPaymentDto>>(recentPayments);

        return dto;
    }
}
