using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.LogSecurityEvent;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class LogSecurityEventCommandHandler : IRequestHandler<LogSecurityEventCommand, AccountSecurityEventDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LogSecurityEventCommandHandler> _logger;

    public LogSecurityEventCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<LogSecurityEventCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AccountSecurityEventDto> Handle(LogSecurityEventCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security event loglanıyor. UserId: {UserId}, EventType: {EventType}, Severity: {Severity}",
            request.UserId, request.EventType, request.Severity);

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // Parse enums
        var eventType = Enum.TryParse<SecurityEventType>(request.EventType, true, out var parsedEventType)
            ? parsedEventType
            : throw new BusinessException($"Invalid EventType: {request.EventType}");

        var severity = Enum.TryParse<SecurityEventSeverity>(request.Severity, true, out var parsedSeverity)
            ? parsedSeverity
            : SecurityEventSeverity.Info;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var securityEvent = AccountSecurityEvent.Create(
            userId: request.UserId,
            eventType: eventType,
            severity: severity,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent,
            location: request.Location,
            deviceFingerprint: request.DeviceFingerprint,
            isSuspicious: request.IsSuspicious,
            details: request.Details != null ? JsonSerializer.Serialize(request.Details) : null,
            requiresAction: request.RequiresAction);

        await _context.Set<AccountSecurityEvent>().AddAsync(securityEvent, cancellationToken);

        // If suspicious, create alert
        if (request.IsSuspicious || request.RequiresAction)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var alertSeverity = severity == SecurityEventSeverity.Critical
                ? AlertSeverity.Critical
                : (severity == SecurityEventSeverity.Warning ? AlertSeverity.High : AlertSeverity.Medium);

            // ✅ SECURITY FIX: Email'i loglama - PII exposure riski, sadece UserId kullan
            var alert = SecurityAlert.Create(
                alertType: AlertType.Account,
                title: $"Suspicious activity detected: {request.EventType}",
                description: $"Security event: {request.EventType} for user ID: {request.UserId}",
                severity: alertSeverity,
                userId: request.UserId,
                metadata: request.Details != null ? JsonSerializer.Serialize(request.Details) : null);
            await _context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için Cartesian Explosion önleme
        securityEvent = await _context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .FirstOrDefaultAsync(e => e.Id == securityEvent.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Security event loglandı. EventId: {EventId}, UserId: {UserId}, EventType: {EventType}",
            securityEvent!.Id, request.UserId, request.EventType);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<AccountSecurityEventDto>(securityEvent);
    }
}
