using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.CreateSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSubscriptionPlanCommandHandler> _logger;

    public CreateSubscriptionPlanCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateSubscriptionPlanCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubscriptionPlanDto> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating subscription plan. Name: {Name}, PlanType: {PlanType}, Price: {Price}",
            request.Name, request.PlanType, request.Price);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var plan = SubscriptionPlan.Create(
            name: request.Name,
            description: request.Description,
            planType: request.PlanType,
            price: request.Price,
            durationDays: request.DurationDays,
            billingCycle: request.BillingCycle,
            maxUsers: request.MaxUsers,
            trialDays: request.TrialDays,
            setupFee: request.SetupFee,
            currency: request.Currency,
            features: request.Features != null ? JsonSerializer.Serialize(request.Features) : null,
            isActive: request.IsActive,
            displayOrder: request.DisplayOrder);

        await _context.Set<SubscriptionPlan>().AddAsync(plan, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes for mapping
        plan = await _context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == plan.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Subscription plan created successfully. PlanId: {PlanId}, Name: {Name}",
            plan!.Id, plan.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<SubscriptionPlanDto>(plan);
        
        // ✅ PERFORMANCE: Batch load subscriber count
        var subscriberCount = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .CountAsync(us => us.SubscriptionPlanId == plan.Id && 
                            (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial), 
                            cancellationToken);
        
        dto.SubscriberCount = subscriberCount;
        
        return dto;
    }
}
