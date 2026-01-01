using AutoMapper;
using NotificationEntity = Merge.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Notification;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Notification;
using Merge.Application.Common;


namespace Merge.Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly IRepository<NotificationEntity> _notificationRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IRepository<NotificationEntity> notificationRepository,
        ApplicationDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    // ✅ PERFORMANCE: Pagination ekle (BEST_PRACTICES_ANALIZI.md - BOLUM 3.1.4)
    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !n.IsDeleted (Global Query Filter)
        IQueryable<NotificationEntity> query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

        return new PagedResult<NotificationDto>
        {
            Items = notificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new NotificationEntity
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            Link = dto.Link
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
        
        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _notificationRepository.UpdateAsync(notification);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası Any() YASAK - List.Count kullan
        if (notifications.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return false;
        }

        await _notificationRepository.DeleteAsync(notification);
        return true;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !n.IsDeleted (Global Query Filter)
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}

