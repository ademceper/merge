using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.TrackUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class TrackUsageCommandHandler : IRequestHandler<TrackUsageCommand, SubscriptionUsageDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TrackUsageCommandHandler> _logger;

    public TrackUsageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TrackUsageCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubscriptionUsageDto> Handle(TrackUsageCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Tracking subscription usage. SubscriptionId: {SubscriptionId}, Feature: {Feature}, Count: {Count}",
            request.UserSubscriptionId, request.Feature, request.Count);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // ✅ NOT: AsNoTracking() YOK - Subscription nesnesine ihtiyaç var (Create method için)
        var subscription = await _context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.UserSubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.UserSubscriptionId);
        }

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        // ✅ NOT: Include UserSubscription - Domain event'te UserId'ye ihtiyaç var
        var usage = await _context.Set<SubscriptionUsage>()
            .Include(u => u.UserSubscription)
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == request.UserSubscriptionId &&
                                     u.Feature == request.Feature &&
                                     u.PeriodStart == periodStart, cancellationToken);

        if (usage == null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            usage = SubscriptionUsage.Create(
                subscription: subscription,
                feature: request.Feature,
                periodStart: periodStart,
                periodEnd: periodEnd);
            
            await _context.Set<SubscriptionUsage>().AddAsync(usage, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        usage.IncrementUsage(request.Count);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<SubscriptionUsageDto>(usage);
    }
}
