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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateSecurityAlertCommandHandler : IRequestHandler<CreateSecurityAlertCommand, SecurityAlertDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSecurityAlertCommandHandler> _logger;

    public CreateSecurityAlertCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateSecurityAlertCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SecurityAlertDto> Handle(CreateSecurityAlertCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Security alert oluşturuluyor. UserId: {UserId}, AlertType: {AlertType}, Severity: {Severity}",
            request.UserId, request.AlertType, request.Severity);

        var severity = Enum.TryParse<AlertSeverity>(request.Severity, true, out var parsedSeverity)
            ? parsedSeverity
            : AlertSeverity.Medium;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        var alert = SecurityAlert.Create(
            alertType: request.AlertType,
            title: request.Title,
            description: request.Description,
            severity: severity,
            userId: request.UserId,
            metadata: request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null);

        await _context.Set<SecurityAlert>().AddAsync(alert, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için Cartesian Explosion önleme
        alert = await _context.Set<SecurityAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .FirstOrDefaultAsync(a => a.Id == alert.Id, cancellationToken);

        _logger.LogInformation("Security alert oluşturuldu. AlertId: {AlertId}, UserId: {UserId}", alert!.Id, request.UserId);

        return _mapper.Map<SecurityAlertDto>(alert);
    }
}
