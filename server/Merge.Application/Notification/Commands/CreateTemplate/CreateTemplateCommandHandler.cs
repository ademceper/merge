using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.CreateTemplate;

/// <summary>
/// Create Template Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreateTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTemplateCommandHandler> logger) : IRequestHandler<CreateTemplateCommand, NotificationTemplateDto>
{

    public async Task<NotificationTemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification template oluşturuluyor. Name: {Name}, Type: {Type}",
            request.Dto.Name, request.Dto.Type);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var template = NotificationTemplate.Create(
            request.Dto.Name,
            request.Dto.Type,
            request.Dto.TitleTemplate,
            request.Dto.MessageTemplate,
            request.Dto.Description,
            request.Dto.LinkTemplate,
            request.Dto.IsActive,
            request.Dto.Variables != null ? JsonSerializer.Serialize(request.Dto.Variables) : null,
            request.Dto.DefaultData != null ? JsonSerializer.Serialize(request.Dto.DefaultData) : null);

        await context.Set<NotificationTemplate>().AddAsync(template, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification template oluşturuldu. TemplateId: {TemplateId}, Name: {Name}",
            template.Id, request.Dto.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationTemplateDto>(template);
    }
}
