using AutoMapper;
using NotificationEntity = Merge.Domain.Modules.Notifications.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;
using Merge.Application.Configuration;
using System.Linq;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Notifications;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Notifications.Notification>;

namespace Merge.Application.Services.Notification;

public class NotificationService(IRepository notificationRepository, IDbContext context, IMapper mapper, IUnitOfWork unitOfWork, ILogger<NotificationService> logger, IOptions<PaginationSettings> paginationSettings) : INotificationService
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<NotificationEntity> query = context.Set<NotificationEntity>()
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var notificationDtos = mapper.Map<IEnumerable<NotificationDto>>(notifications);

        return new PagedResult<NotificationDto>
        {
            Items = notificationDtos.ToList(), // ✅ IEnumerable -> List'e çevir
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<NotificationDto?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Set<NotificationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

        if (notification == null)
        {
            return null;
        }

        return mapper.Map<NotificationDto>(notification);
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Notification oluşturuluyor. UserId: {UserId}, Type: {Type}, Title: {Title}",
            dto.UserId, dto.Type, dto.Title);

        var notification = NotificationEntity.Create(
            dto.UserId,
            dto.Type,
            dto.Title,
            dto.Message,
            dto.Link,
            dto.Data);

        await notificationRepository.AddAsync(notification);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation(
            "Notification oluşturuldu. NotificationId: {NotificationId}, UserId: {UserId}, Type: {Type}",
            notification.Id, dto.UserId, dto.Type);
        
        return mapper.Map<NotificationDto>(notification);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        notification.MarkAsRead();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await context.Set<NotificationEntity>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        if (notifications.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        notification.Delete();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Set<NotificationEntity>()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }
}

