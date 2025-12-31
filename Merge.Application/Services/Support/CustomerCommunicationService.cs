using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using UserEntity = Merge.Domain.Entities.User;
using System.Text.Json;
using Merge.Application.DTOs.Support;


namespace Merge.Application.Services.Support;

public class CustomerCommunicationService : ICustomerCommunicationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerCommunicationService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerCommunicationDto> CreateCommunicationAsync(CreateCustomerCommunicationDto dto, Guid? sentByUserId = null)
    {
        var communication = new CustomerCommunication
        {
            UserId = dto.UserId,
            CommunicationType = dto.CommunicationType,
            Channel = dto.Channel,
            Subject = dto.Subject,
            Content = dto.Content,
            Direction = dto.Direction,
            RelatedEntityId = dto.RelatedEntityId,
            RelatedEntityType = dto.RelatedEntityType,
            SentByUserId = sentByUserId,
            RecipientEmail = dto.RecipientEmail,
            RecipientPhone = dto.RecipientPhone,
            Status = "Sent",
            SentAt = DateTime.UtcNow,
            Metadata = dto.Metadata != null ? JsonSerializer.Serialize(dto.Metadata) : null
        };

        await _context.Set<CustomerCommunication>().AddAsync(communication);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(communication);
    }

    public async Task<CustomerCommunicationDto?> GetCommunicationAsync(Guid id)
    {
        var communication = await _context.Set<CustomerCommunication>()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return communication != null ? await MapToDto(communication) : null;
    }

    public async Task<IEnumerable<CustomerCommunicationDto>> GetUserCommunicationsAsync(Guid userId, string? communicationType = null, string? channel = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<CustomerCommunication>()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.UserId == userId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(communicationType))
        {
            query = query.Where(c => c.CommunicationType == communicationType);
        }

        if (!string.IsNullOrEmpty(channel))
        {
            query = query.Where(c => c.Channel == channel);
        }

        var communications = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<CustomerCommunicationDto>();
        foreach (var comm in communications)
        {
            result.Add(await MapToDto(comm));
        }
        return result;
    }

    public async Task<CommunicationHistoryDto> GetUserCommunicationHistoryAsync(Guid userId)
    {
        var user = await _context.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        var communications = await _context.Set<CustomerCommunication>()
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync();

        var history = new CommunicationHistoryDto
        {
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            TotalCommunications = communications.Count,
            CommunicationsByType = communications
                .GroupBy(c => c.CommunicationType)
                .ToDictionary(g => g.Key, g => g.Count()),
            CommunicationsByChannel = communications
                .GroupBy(c => c.Channel)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastCommunicationDate = communications.Any() 
                ? communications.Max(c => c.CreatedAt) 
                : null
        };

        // Get recent communications
        var recent = await _context.Set<CustomerCommunication>()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        history.RecentCommunications = new List<CustomerCommunicationDto>();
        foreach (var comm in recent)
        {
            history.RecentCommunications.Add(await MapToDto(comm));
        }

        return history;
    }

    public async Task<IEnumerable<CustomerCommunicationDto>> GetAllCommunicationsAsync(string? communicationType = null, string? channel = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<CustomerCommunication>()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrEmpty(communicationType))
        {
            query = query.Where(c => c.CommunicationType == communicationType);
        }

        if (!string.IsNullOrEmpty(channel))
        {
            query = query.Where(c => c.Channel == channel);
        }

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        var communications = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<CustomerCommunicationDto>();
        foreach (var comm in communications)
        {
            result.Add(await MapToDto(comm));
        }
        return result;
    }

    public async Task<bool> UpdateCommunicationStatusAsync(Guid id, string status, DateTime? deliveredAt = null, DateTime? readAt = null)
    {
        var communication = await _context.Set<CustomerCommunication>()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (communication == null) return false;

        communication.Status = status;
        if (deliveredAt.HasValue)
            communication.DeliveredAt = deliveredAt.Value;
        if (readAt.HasValue)
            communication.ReadAt = readAt.Value;
        
        communication.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Dictionary<string, int>> GetCommunicationStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<CustomerCommunication>()
            .Where(c => !c.IsDeleted);

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        var communications = await query.ToListAsync();

        return new Dictionary<string, int>
        {
            { "Total", communications.Count },
            { "Email", communications.Count(c => c.CommunicationType == "Email") },
            { "SMS", communications.Count(c => c.CommunicationType == "SMS") },
            { "Ticket", communications.Count(c => c.CommunicationType == "Ticket") },
            { "InApp", communications.Count(c => c.CommunicationType == "InApp") },
            { "Sent", communications.Count(c => c.Status == "Sent") },
            { "Delivered", communications.Count(c => c.Status == "Delivered") },
            { "Read", communications.Count(c => c.Status == "Read") },
            { "Failed", communications.Count(c => c.Status == "Failed") }
        };
    }

    private async Task<CustomerCommunicationDto> MapToDto(CustomerCommunication communication)
    {
        await _context.Entry(communication)
            .Reference(c => c.User)
            .LoadAsync();
        await _context.Entry(communication)
            .Reference(c => c.SentBy)
            .LoadAsync();

        return new CustomerCommunicationDto
        {
            Id = communication.Id,
            UserId = communication.UserId,
            UserName = communication.User != null 
                ? $"{communication.User.FirstName} {communication.User.LastName}" 
                : string.Empty,
            CommunicationType = communication.CommunicationType,
            Channel = communication.Channel,
            Subject = communication.Subject,
            Content = communication.Content,
            Direction = communication.Direction,
            RelatedEntityId = communication.RelatedEntityId,
            RelatedEntityType = communication.RelatedEntityType,
            SentByUserId = communication.SentByUserId,
            SentByName = communication.SentBy != null 
                ? $"{communication.SentBy.FirstName} {communication.SentBy.LastName}" 
                : null,
            RecipientEmail = communication.RecipientEmail,
            RecipientPhone = communication.RecipientPhone,
            Status = communication.Status,
            SentAt = communication.SentAt,
            DeliveredAt = communication.DeliveredAt,
            ReadAt = communication.ReadAt,
            ErrorMessage = communication.ErrorMessage,
            Metadata = !string.IsNullOrEmpty(communication.Metadata)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(communication.Metadata)
                : null,
            CreatedAt = communication.CreatedAt
        };
    }
}

