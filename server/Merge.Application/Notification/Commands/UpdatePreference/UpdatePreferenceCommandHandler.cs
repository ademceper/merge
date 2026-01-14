using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces;
using Merge.Application.DTOs.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Notification.Commands.UpdatePreference;

/// <summary>
/// Update Preference Command Handler - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UpdatePreferenceCommandHandler : IRequestHandler<UpdatePreferenceCommand, NotificationPreferenceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePreferenceCommandHandler> _logger;

    public UpdatePreferenceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdatePreferenceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NotificationPreferenceDto> Handle(UpdatePreferenceCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !np.IsDeleted (Global Query Filter)
        var preference = await _context.Set<NotificationPreference>()
            .FirstOrDefaultAsync(np => np.UserId == request.UserId && 
                                      np.NotificationType == request.NotificationType && 
                                      np.Channel == request.Channel, cancellationToken);

        if (preference == null)
        {
            throw new NotFoundException("Tercih", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        preference.Update(
            request.Dto.IsEnabled ?? preference.IsEnabled,
            request.Dto.CustomSettings != null ? JsonSerializer.Serialize(request.Dto.CustomSettings) : preference.CustomSettings);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Notification preference güncellendi. UserId: {UserId}, NotificationType: {NotificationType}, Channel: {Channel}",
            request.UserId, request.NotificationType, request.Channel);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationPreferenceDto>(preference);
    }
}
