using AutoMapper;
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
    private readonly IMapper _mapper;

    public CustomerCommunicationService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        // ✅ PERFORMANCE: Reload with includes for mapping
        communication = await _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .FirstOrDefaultAsync(c => c.Id == communication.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<CustomerCommunicationDto>(communication!);
    }

    public async Task<CustomerCommunicationDto?> GetCommunicationAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var communication = await _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .FirstOrDefaultAsync(c => c.Id == id);

        return communication != null ? _mapper.Map<CustomerCommunicationDto>(communication) : null;
    }

    public async Task<IEnumerable<CustomerCommunicationDto>> GetUserCommunicationsAsync(Guid userId, string? communicationType = null, string? channel = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var query = _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.UserId == userId);

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

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<CustomerCommunicationDto>>(communications);
    }

    public async Task<CommunicationHistoryDto> GetUserCommunicationHistoryAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var user = await _context.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", userId);
        }

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        var totalCommunications = await query.CountAsync();
        var communicationsByType = await query
            .GroupBy(c => c.CommunicationType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);
        var communicationsByChannel = await query
            .GroupBy(c => c.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count);
        var lastCommunicationDate = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => (DateTime?)c.CreatedAt)
            .FirstOrDefaultAsync();

        // Get recent communications
        var recent = await query
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        var history = new CommunicationHistoryDto
        {
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            TotalCommunications = totalCommunications,
            CommunicationsByType = communicationsByType,
            CommunicationsByChannel = communicationsByChannel,
            LastCommunicationDate = lastCommunicationDate,
            RecentCommunications = _mapper.Map<List<CustomerCommunicationDto>>(recent)
        };

        return history;
    }

    public async Task<IEnumerable<CustomerCommunicationDto>> GetAllCommunicationsAsync(string? communicationType = null, string? channel = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<CustomerCommunication> query = _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.SentBy);

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

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<CustomerCommunicationDto>>(communications);
    }

    public async Task<bool> UpdateCommunicationStatusAsync(Guid id, string status, DateTime? deliveredAt = null, DateTime? readAt = null)
    {
        // ✅ PERFORMANCE: Global Query Filter otomatik uygulanır, manuel !IsDeleted kontrolü YASAK
        var communication = await _context.Set<CustomerCommunication>()
            .FirstOrDefaultAsync(c => c.Id == id);

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
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<CustomerCommunication>()
            .AsNoTracking();

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        var total = await query.CountAsync();
        var email = await query.CountAsync(c => c.CommunicationType == "Email");
        var sms = await query.CountAsync(c => c.CommunicationType == "SMS");
        var ticket = await query.CountAsync(c => c.CommunicationType == "Ticket");
        var inApp = await query.CountAsync(c => c.CommunicationType == "InApp");
        var sent = await query.CountAsync(c => c.Status == "Sent");
        var delivered = await query.CountAsync(c => c.Status == "Delivered");
        var read = await query.CountAsync(c => c.Status == "Read");
        var failed = await query.CountAsync(c => c.Status == "Failed");

        return new Dictionary<string, int>
        {
            { "Total", total },
            { "Email", email },
            { "SMS", sms },
            { "Ticket", ticket },
            { "InApp", inApp },
            { "Sent", sent },
            { "Delivered", delivered },
            { "Read", read },
            { "Failed", failed }
        };
    }

}

