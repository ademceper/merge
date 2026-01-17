using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.UpdateTemplate;

/// <summary>
/// Update Template Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UpdateTemplateCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTemplateCommandHandler> logger) : IRequestHandler<UpdateTemplateCommand, NotificationTemplateDto>
{

    public async Task<NotificationTemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var template = await context.Set<NotificationTemplate>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            throw new NotFoundException("Şablon", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        template.Update(
            request.Dto.Name,
            request.Dto.Description,
            request.Dto.Type,
            request.Dto.TitleTemplate,
            request.Dto.MessageTemplate,
            request.Dto.LinkTemplate,
            request.Dto.IsActive,
            request.Dto.Variables != null ? JsonSerializer.Serialize(request.Dto.Variables) : null,
            request.Dto.DefaultData != null ? JsonSerializer.Serialize(request.Dto.DefaultData) : null);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Notification template güncellendi. TemplateId: {TemplateId}",
            request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<NotificationTemplateDto>(template);
    }
}
