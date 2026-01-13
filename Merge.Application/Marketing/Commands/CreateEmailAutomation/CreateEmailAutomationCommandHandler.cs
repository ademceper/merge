using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CreateEmailAutomation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CreateEmailAutomationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateEmailAutomationCommandHandler> logger) : IRequestHandler<CreateEmailAutomationCommand, EmailAutomationDto>
{
    public async Task<EmailAutomationDto> Handle(CreateEmailAutomationCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email otomasyonu oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Name, request.Type);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var typeEnum = Enum.Parse<EmailAutomationType>(request.Type, true);
        var automation = EmailAutomation.Create(
            name: request.Name,
            description: request.Description,
            type: typeEnum,
            templateId: request.TemplateId,
            delayHours: request.DelayHours,
            triggerConditions: request.TriggerConditions != null ? JsonSerializer.Serialize(request.TriggerConditions) : null);

        await context.Set<EmailAutomation>().AddAsync(automation, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + AsSplitQuery + Removed manual !a.IsDeleted (Global Query Filter)
        var createdAutomation = await context.Set<EmailAutomation>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Template)
            .FirstOrDefaultAsync(a => a.Id == automation.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email otomasyonu oluşturuldu. AutomationId: {AutomationId}, Name: {Name}",
            automation.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<EmailAutomationDto>(createdAutomation!);
    }
}
