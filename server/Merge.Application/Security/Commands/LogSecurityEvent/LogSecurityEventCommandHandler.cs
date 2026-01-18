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

public class LogSecurityEventCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LogSecurityEventCommandHandler> logger) : IRequestHandler<LogSecurityEventCommand, AccountSecurityEventDto>
{

    public async Task<AccountSecurityEventDto> Handle(LogSecurityEventCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Security event loglanıyor. UserId: {UserId}, EventType: {EventType}, Severity: {Severity}",
            request.UserId, request.EventType, request.Severity);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
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

        var securityEvent = AccountSecurityEvent.Create(
            userId: request.UserId,
            eventType: eventType,
            severity: severity,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent,
            location: request.Location,
            deviceFingerprint: request.DeviceFingerprint,
            isSuspicious: request.IsSuspicious,
            details: request.Details is not null ? JsonSerializer.Serialize(request.Details) : null,
            requiresAction: request.RequiresAction);

        await context.Set<AccountSecurityEvent>().AddAsync(securityEvent, cancellationToken);

        // If suspicious, create alert
        if (request.IsSuspicious || request.RequiresAction)
        {
            var alertSeverity = severity == SecurityEventSeverity.Critical
                ? AlertSeverity.Critical
                : (severity == SecurityEventSeverity.Warning ? AlertSeverity.High : AlertSeverity.Medium);

            var alert = SecurityAlert.Create(
                alertType: AlertType.Account,
                title: $"Suspicious activity detected: {request.EventType}",
                description: $"Security event: {request.EventType} for user ID: {request.UserId}",
                severity: alertSeverity,
                userId: request.UserId,
                metadata: request.Details is not null ? JsonSerializer.Serialize(request.Details) : null);
            await context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        securityEvent = await context.Set<AccountSecurityEvent>()
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.ActionTakenBy)
            .FirstOrDefaultAsync(e => e.Id == securityEvent.Id, cancellationToken);

        logger.LogInformation("Security event loglandı. EventId: {EventId}, UserId: {UserId}, EventType: {EventType}",
            securityEvent!.Id, request.UserId, request.EventType);

        return mapper.Map<AccountSecurityEventDto>(securityEvent);
    }
}
