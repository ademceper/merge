using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Commands.LogActivity;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class LogActivityCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<LogActivityCommandHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<LogActivityCommand>
{
    private readonly UserSettings config = userSettings.Value;


    public async Task Handle(LogActivityCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        logger.LogDebug("Logging activity: {ActivityType} for user: {UserId}", request.ActivityType, request.UserId);

        var deviceInfo = ParseUserAgent(request.UserAgent);

        // Parse enum values from strings
        if (!Enum.TryParse<ActivityType>(request.ActivityType, true, out var activityType))
        {
            logger.LogWarning("Invalid ActivityType: {ActivityType}", request.ActivityType);
            throw new ArgumentException($"Invalid ActivityType: {request.ActivityType}", nameof(request.ActivityType));
        }

        if (!Enum.TryParse<EntityType>(request.EntityType, true, out var entityType))
        {
            logger.LogWarning("Invalid EntityType: {EntityType}", request.EntityType);
            throw new ArgumentException($"Invalid EntityType: {request.EntityType}", nameof(request.EntityType));
        }

        DeviceType deviceType = DeviceType.Other;
        if (!string.IsNullOrEmpty(deviceInfo.DeviceType))
        {
            if (!Enum.TryParse<DeviceType>(deviceInfo.DeviceType, true, out deviceType))
                deviceType = DeviceType.Other;
        }
                // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var activity = UserActivityLog.Create(
            activityType: activityType,
            entityType: entityType,
            description: request.Description,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent,
            userId: request.UserId,
            entityId: request.EntityId,
            deviceType: deviceType,
            browser: deviceInfo.Browser,
            os: deviceInfo.OS,
            metadata: request.Metadata,
            durationMs: request.DurationMs,
            wasSuccessful: request.WasSuccessful,
            errorMessage: request.ErrorMessage);

        await context.Set<UserActivityLog>().AddAsync(activity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event\'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır

        logger.LogDebug("Activity logged successfully with ID: {ActivityId}", activity.Id);
    }

    private (string DeviceType, string Browser, string OS) ParseUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return ("Unknown", "Unknown", "Unknown");
        }

        var deviceType = "Desktop";
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            deviceType = "Mobile";
        else if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            deviceType = "Tablet";

        var browser = "Unknown";
        if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
            browser = "Chrome";
        else if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
            browser = "Firefox";
        else if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
            browser = "Safari";
        else if (userAgent.Contains("Edge", StringComparison.OrdinalIgnoreCase))
            browser = "Edge";

        var os = "Unknown";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            os = "Windows";
        else if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase))
            os = "macOS";
        else if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
            os = "Linux";
        else if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
            os = "Android";
        else if (userAgent.Contains("iOS", StringComparison.OrdinalIgnoreCase) ||
                 userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
            os = "iOS";

        return (deviceType, browser, os);
    }
}
