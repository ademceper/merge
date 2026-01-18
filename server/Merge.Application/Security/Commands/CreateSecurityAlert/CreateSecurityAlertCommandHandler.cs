using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.CreateSecurityAlert;

public class CreateSecurityAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateSecurityAlertCommandHandler> logger) : IRequestHandler<CreateSecurityAlertCommand, SecurityAlertDto>
{

    public async Task<SecurityAlertDto> Handle(CreateSecurityAlertCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Security alert oluşturuluyor. UserId: {UserId}, AlertType: {AlertType}, Severity: {Severity}",
            request.UserId, request.AlertType, request.Severity);

        var severity = Enum.TryParse<AlertSeverity>(request.Severity, true, out var parsedSeverity)
            ? parsedSeverity
            : AlertSeverity.Medium;

        // Parse AlertType from string to enum
        if (!Enum.TryParse<AlertType>(request.AlertType, true, out var alertType))
        {
            logger.LogWarning("Invalid AlertType: {AlertType}, defaulting to Other", request.AlertType);
            alertType = AlertType.Other;
        }

        var alert = SecurityAlert.Create(
            alertType: alertType,
            title: request.Title,
            description: request.Description,
            severity: severity,
            userId: request.UserId,
            metadata: request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null);

        await context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        alert = await context.Set<SecurityAlert>()
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        logger.LogInformation("Security alert oluşturuldu. AlertId: {AlertId}, UserId: {UserId}", alert!.Id, request.UserId);

        return mapper.Map<SecurityAlertDto>(alert);
    }
}
