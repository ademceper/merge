using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Support;


namespace Merge.Application.Services.Support;

public class SupportTicketService : ISupportTicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupportTicketService> _logger;

    public SupportTicketService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        ILogger<SupportTicketService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SupportTicketDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto)
    {
        _logger.LogInformation("Creating support ticket for user {UserId}. Category: {Category}, Priority: {Priority}, Subject: {Subject}",
            userId, dto.Category, dto.Priority, dto.Subject);

        var user = await _context.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found while creating support ticket", userId);
            throw new NotFoundException("Kullanıcı", userId);
        }

        // Generate ticket number
        var ticketNumber = await GenerateTicketNumberAsync();

        var ticket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            UserId = userId,
            Category = Enum.Parse<TicketCategory>(dto.Category, true),
            Priority = Enum.Parse<TicketPriority>(dto.Priority, true),
            Subject = dto.Subject,
            Description = dto.Description,
            OrderId = dto.OrderId,
            ProductId = dto.ProductId
        };

        await _context.Set<SupportTicket>().AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Support ticket {TicketNumber} created successfully for user {UserId}", ticketNumber, userId);

        // Send confirmation email
        try
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                $"Support Ticket Created - {ticketNumber}",
                $"Your support ticket has been created. Ticket Number: {ticketNumber}. Subject: {dto.Subject}. We'll respond as soon as possible."
            );
            _logger.LogInformation("Confirmation email sent for ticket {TicketNumber}", ticketNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for ticket {TicketNumber}", ticketNumber);
        }

        // ✅ PERFORMANCE: Reload with includes for mapping
        ticket = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id);

        return await MapToDtoAsync(ticket!);
    }

    public async Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, Guid? userId = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .Include(t => t.Attachments)
            .Where(t => t.Id == ticketId);

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        var ticket = await query.FirstOrDefaultAsync();

        return ticket != null ? await MapToDtoAsync(ticket) : null;
    }

    public async Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .Include(t => t.Attachments)
            .Where(t => t.TicketNumber == ticketNumber);

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        var ticket = await query.FirstOrDefaultAsync();

        return ticket != null ? await MapToDtoAsync(ticket) : null;
    }

    // ✅ PERFORMANCE: Pagination eklendi - unbounded query önleme
    public async Task<IEnumerable<SupportTicketDto>> GetUserTicketsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            var ticketStatus = Enum.Parse<TicketStatus>(status, true);
            query = query.Where(t => t.Status == ticketStatus);
        }

        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        // ✅ ticketIds query'sinde de aynı filtreleri uygula
        var ticketIdsQuery = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            var ticketStatus = Enum.Parse<TicketStatus>(status, true);
            ticketIdsQuery = ticketIdsQuery.Where(t => t.Status == ticketStatus);
        }

        // ✅ PERFORMANCE: Pagination uygula
        var ticketIds = await ticketIdsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load messages and attachments for all tickets

        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments);

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<SupportTicketDto>> GetAllTicketsAsync(string? status = null, string? category = null, Guid? assignedToId = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            var ticketStatus = Enum.Parse<TicketStatus>(status, true);
            query = query.Where(t => t.Status == ticketStatus);
        }

        if (!string.IsNullOrEmpty(category))
        {
            var ticketCategory = Enum.Parse<TicketCategory>(category, true);
            query = query.Where(t => t.Category == ticketCategory);
        }

        if (assignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == assignedToId.Value);
        }

        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync();

        var tickets = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load messages and attachments for all tickets
        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments);

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> UpdateTicketAsync(Guid ticketId, UpdateSupportTicketDto dto)
    {
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for update", ticketId);
            return false;
        }

        var oldStatus = ticket.Status;

        if (!string.IsNullOrEmpty(dto.Category))
        {
            ticket.Category = Enum.Parse<TicketCategory>(dto.Category, true);
        }

        if (!string.IsNullOrEmpty(dto.Priority))
        {
            ticket.Priority = Enum.Parse<TicketPriority>(dto.Priority, true);
        }

        if (!string.IsNullOrEmpty(dto.Status))
        {
            var newStatus = Enum.Parse<TicketStatus>(dto.Status, true);
            ticket.Status = newStatus;

            if (newStatus == TicketStatus.Resolved && ticket.ResolvedAt == null)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                _logger.LogInformation("Ticket {TicketNumber} marked as resolved", ticket.TicketNumber);
            }
            else if (newStatus == TicketStatus.Closed && ticket.ClosedAt == null)
            {
                ticket.ClosedAt = DateTime.UtcNow;
                _logger.LogInformation("Ticket {TicketNumber} marked as closed", ticket.TicketNumber);
            }
        }

        if (dto.AssignedToId.HasValue)
        {
            ticket.AssignedToId = dto.AssignedToId.Value;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketNumber} updated. Status changed from {OldStatus} to {NewStatus}",
            ticket.TicketNumber, oldStatus, ticket.Status);

        return true;
    }

    public async Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId)
    {
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for assignment", ticketId);
            return false;
        }

        ticket.AssignedToId = assignedToId;

        if (ticket.Status == TicketStatus.Open)
        {
            ticket.Status = TicketStatus.InProgress;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketNumber} assigned to user {AssignedToId}", ticket.TicketNumber, assignedToId);

        return true;
    }

    public async Task<bool> CloseTicketAsync(Guid ticketId)
    {
        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for closure", ticketId);
            return false;
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketNumber} closed successfully", ticket.TicketNumber);

        // Send closure email
        try
        {
            await _emailService.SendEmailAsync(
                ticket.User?.Email ?? string.Empty,
                $"Ticket Closed - {ticket.TicketNumber}",
                $"Your support ticket #{ticket.TicketNumber} has been closed. If you need further assistance, please open a new ticket."
            );
            _logger.LogInformation("Closure email sent for ticket {TicketNumber}", ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send closure email for ticket {TicketNumber}", ticket.TicketNumber);
        }

        return true;
    }

    public async Task<bool> ReopenTicketAsync(Guid ticketId)
    {
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for reopening", ticketId);
            return false;
        }

        ticket.Status = TicketStatus.Open;
        ticket.ClosedAt = null;
        ticket.ResolvedAt = null;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketNumber} reopened", ticket.TicketNumber);

        return true;
    }

    public async Task<TicketMessageDto> AddMessageAsync(Guid userId, CreateTicketMessageDto dto, bool isStaffResponse = false)
    {
        _logger.LogInformation("Adding message to ticket {TicketId} from user {UserId}. IsStaffResponse: {IsStaffResponse}",
            dto.TicketId, userId, isStaffResponse);

        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == dto.TicketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found while adding message", dto.TicketId);
            throw new NotFoundException("Destek bileti", dto.TicketId);
        }

        var message = new TicketMessage
        {
            TicketId = dto.TicketId,
            UserId = userId,
            Message = dto.Message,
            IsStaffResponse = isStaffResponse,
            IsInternal = dto.IsInternal
        };

        await _context.Set<TicketMessage>().AddAsync(message);

        ticket.ResponseCount++;
        ticket.LastResponseAt = DateTime.UtcNow;

        var oldStatus = ticket.Status;

        if (ticket.Status == TicketStatus.Waiting && isStaffResponse)
        {
            ticket.Status = TicketStatus.InProgress;
        }
        else if (!isStaffResponse)
        {
            ticket.Status = TicketStatus.Waiting;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Message added to ticket {TicketNumber}. Response count: {ResponseCount}, Status: {OldStatus} -> {NewStatus}",
            ticket.TicketNumber, ticket.ResponseCount, oldStatus, ticket.Status);

        // Send email notification
        if (isStaffResponse && !dto.IsInternal)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    ticket.User?.Email ?? string.Empty,
                    $"New Response on Ticket {ticket.TicketNumber}",
                    $"You have received a new response on your support ticket #{ticket.TicketNumber}."
                );
                _logger.LogInformation("Response notification email sent for ticket {TicketNumber}", ticket.TicketNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send response notification for ticket {TicketNumber}", ticket.TicketNumber);
            }
        }

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        message = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<TicketMessageDto>(message!);
    }

    public async Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(Guid ticketId, bool includeInternal = false)
    {
        var query = _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Attachments)
            .Where(m => m.TicketId == ticketId);

        if (!includeInternal)
        {
            query = query.Where(m => !m.IsInternal);
        }

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<TicketMessageDto>>(messages);
    }

    public async Task<TicketStatsDto> GetTicketStatsAsync()
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<SupportTicket>()
            .AsNoTracking();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddMonths(-1);

        var totalTickets = await query.CountAsync();
        var openTickets = await query.CountAsync(t => t.Status == TicketStatus.Open);
        var inProgressTickets = await query.CountAsync(t => t.Status == TicketStatus.InProgress);
        var resolvedTickets = await query.CountAsync(t => t.Status == TicketStatus.Resolved);
        var closedTickets = await query.CountAsync(t => t.Status == TicketStatus.Closed);
        var ticketsToday = await query.CountAsync(t => t.CreatedAt >= today);
        var ticketsThisWeek = await query.CountAsync(t => t.CreatedAt >= weekAgo);
        var ticketsThisMonth = await query.CountAsync(t => t.CreatedAt >= monthAgo);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedTicketsQuery = query.Where(t => t.ResolvedAt.HasValue);
        var avgResolutionTime = await resolvedTicketsQuery.AnyAsync()
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var ticketsWithResponseQuery = query.Where(t => t.LastResponseAt.HasValue && t.ResolvedAt.HasValue);
        var avgResponseTime = await ticketsWithResponseQuery.AnyAsync()
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var ticketsByCategory = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);

        var ticketsByPriority = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        _logger.LogInformation("Ticket stats generated: Total {Total}, Resolved {Resolved}, Avg Resolution Time {AvgTime}h",
            totalTickets, resolvedTickets, Math.Round(avgResolutionTime, 2));

        return new TicketStatsDto
        {
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            ClosedTickets = closedTickets,
            TicketsToday = ticketsToday,
            TicketsThisWeek = ticketsThisWeek,
            TicketsThisMonth = ticketsThisMonth,
            AverageResponseTime = (decimal)Math.Round(avgResponseTime, 2),
            AverageResolutionTime = (decimal)Math.Round(avgResolutionTime, 2),
            TicketsByCategory = ticketsByCategory,
            TicketsByPriority = ticketsByPriority
        };
    }

    public async Task<IEnumerable<SupportTicketDto>> GetUnassignedTicketsAsync()
    {
        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync();

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load messages and attachments for all tickets
        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments);

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<SupportTicketDto>> GetMyAssignedTicketsAsync(Guid agentId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == agentId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync();

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == agentId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load messages and attachments for all tickets
        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments);

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            dtos.Add(dto);
        }

        return dtos;
    }

    private async Task<string> GenerateTicketNumberAsync()
    {
        var lastTicket = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastTicket != null && lastTicket.TicketNumber.StartsWith("TKT-"))
        {
            var numberPart = lastTicket.TicketNumber.Substring(4);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"TKT-{nextNumber:D6}";
    }

    private async Task<SupportTicketDto> MapToDtoAsync(SupportTicket ticket)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan
        var dto = _mapper.Map<SupportTicketDto>(ticket);

        // ✅ PERFORMANCE: Batch load messages and attachments if not already loaded
        if (ticket.Messages == null || ticket.Messages.Count == 0)
        {
            var messages = await _context.Set<TicketMessage>()
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Where(m => m.TicketId == ticket.Id)
                .ToListAsync();
            dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
        }
        else
        {
            dto.Messages = _mapper.Map<List<TicketMessageDto>>(ticket.Messages);
        }

        if (ticket.Attachments == null || ticket.Attachments.Count == 0)
        {
            var attachments = await _context.Set<TicketAttachment>()
                .AsNoTracking()
                .Where(a => a.TicketId == ticket.Id)
                .ToListAsync();
            dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
        }
        else
        {
            dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(ticket.Attachments);
        }

        return dto;
    }

    public async Task<SupportAgentDashboardDto> GetAgentDashboardAsync(Guid agentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        _logger.LogInformation("Generating agent dashboard for agent {AgentId} from {StartDate} to {EndDate}",
            agentId, startDate, endDate);

        var agent = await _context.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == agentId);

        if (agent == null)
        {
            _logger.LogWarning("Agent {AgentId} not found", agentId);
            throw new NotFoundException("Ajan", agentId);
        }

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var allTicketsQuery = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == agentId);

        var totalTickets = await allTicketsQuery.CountAsync();
        var openTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open);
        var inProgressTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.InProgress);
        var resolvedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Resolved);
        var closedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Closed);

        // Unassigned tickets (for admin view)
        var unassignedTickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .CountAsync(t => t.AssignedToId == null && t.Status != TicketStatus.Closed);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedTicketsQuery = allTicketsQuery.Where(t => t.ResolvedAt.HasValue);
        var averageResolutionTime = await resolvedTicketsQuery.AnyAsync()
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var ticketsWithResponseQuery = allTicketsQuery.Where(t => t.LastResponseAt.HasValue);
        var averageResponseTime = await ticketsWithResponseQuery.AnyAsync()
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var ticketsResolvedToday = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today);
        var ticketsResolvedThisWeek = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= weekAgo);
        var ticketsResolvedThisMonth = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= monthAgo);

        var resolutionRate = totalTickets > 0
            ? (decimal)(resolvedTickets + closedTickets) / totalTickets * 100
            : 0;

        // ✅ PERFORMANCE: Database'de count yap
        var activeTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);
        var overdueTickets = await allTicketsQuery.CountAsync(t =>
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
            t.CreatedAt < DateTime.UtcNow.AddDays(-3)); // Tickets older than 3 days
        var highPriorityTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.High);
        var urgentTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.Urgent);

        // Category breakdown
        var ticketsByCategory = await GetTicketsByCategoryAsync(agentId, startDate, endDate);

        // Priority breakdown
        var ticketsByPriority = await GetTicketsByPriorityAsync(agentId, startDate, endDate);

        // Trends
        var trends = await GetTicketTrendsAsync(agentId, startDate, endDate);

        // ✅ PERFORMANCE: Recent tickets - database'de query
        var recentTickets = await allTicketsQuery
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToListAsync();

        // ✅ PERFORMANCE: Urgent tickets - database'de query
        var urgentTicketsList = await allTicketsQuery
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(10)
            .ToListAsync();

        // ✅ PERFORMANCE: allTicketIds'i database'de oluştur, memory'de işlem YASAK
        // Recent tickets için ID'leri database'de al
        var recentTicketIds = await allTicketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => t.Id)
            .ToListAsync();
        
        // Urgent tickets için ID'leri database'de al
        var urgentTicketIds = await allTicketsQuery
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(10)
            .Select(t => t.Id)
            .ToListAsync();
        
        // ✅ Memory'de minimal işlem: Concat ve Distinct küçük listeler için kabul edilebilir
        // Ancak database'de UNION kullanmak daha iyi olurdu, ancak EF Core'da UNION ile Distinct kombinasyonu karmaşık
        // Bu durumda küçük listeler (max 20 ID) için memory'de Concat+Distinct kabul edilebilir
        var allTicketIds = recentTicketIds.Concat(urgentTicketIds).Distinct().ToList();
        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => allTicketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => allTicketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments);

        var recentTicketsDto = new List<SupportTicketDto>();
        foreach (var ticket in recentTickets)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            recentTicketsDto.Add(dto);
        }

        var urgentTicketsDto = new List<SupportTicketDto>();
        foreach (var ticket in urgentTicketsList)
        {
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            if (messagesDict.TryGetValue(ticket.Id, out var messages))
            {
                dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
            }
            else
            {
                dto.Messages = new List<TicketMessageDto>();
            }
            
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachments))
            {
                dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
            }
            else
            {
                dto.Attachments = new List<TicketAttachmentDto>();
            }
            
            urgentTicketsDto.Add(dto);
        }

        _logger.LogInformation("Agent dashboard generated for {AgentName}. Total tickets: {Total}, Active: {Active}, Resolution rate: {Rate}%",
            $"{agent.FirstName} {agent.LastName}", totalTickets, activeTickets, Math.Round(resolutionRate, 2));

        return new SupportAgentDashboardDto
        {
            AgentId = agentId,
            AgentName = $"{agent.FirstName} {agent.LastName}",
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            ClosedTickets = closedTickets,
            UnassignedTickets = unassignedTickets,
            AverageResponseTime = Math.Round((decimal)averageResponseTime, 2),
            AverageResolutionTime = Math.Round((decimal)averageResolutionTime, 2),
            TicketsResolvedToday = ticketsResolvedToday,
            TicketsResolvedThisWeek = ticketsResolvedThisWeek,
            TicketsResolvedThisMonth = ticketsResolvedThisMonth,
            ResolutionRate = Math.Round(resolutionRate, 2),
            CustomerSatisfactionScore = 0, // Would need feedback system
            ActiveTickets = activeTickets,
            OverdueTickets = overdueTickets,
            HighPriorityTickets = highPriorityTickets,
            UrgentTickets = urgentTickets,
            TicketsByCategory = ticketsByCategory,
            TicketsByPriority = ticketsByPriority,
            TicketTrends = trends,
            RecentTickets = recentTicketsDto,
            UrgentTicketsList = urgentTicketsDto
        };
    }

    public async Task<List<CategoryTicketCountDto>> GetTicketsByCategoryAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .AsQueryable();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        // ✅ PERFORMANCE: Database'de grouping yap, memory'de işlem YASAK
        var total = await query.CountAsync();

        var grouped = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new CategoryTicketCountDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        return grouped;
    }

    public async Task<List<PriorityTicketCountDto>> GetTicketsByPriorityAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .AsQueryable();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        // ✅ PERFORMANCE: Database'de grouping yap, memory'de işlem YASAK
        var total = await query.CountAsync();

        var grouped = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityTicketCountDto
            {
                Priority = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(p => p.Count)
            .ToListAsync();

        return grouped;
    }

    public async Task<List<TicketTrendDto>> GetTicketTrendsAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .AsQueryable();

        if (agentId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == agentId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        // ✅ PERFORMANCE: Database'de grouping yap, memory'de işlem YASAK
        // Note: EF Core doesn't support grouping by Date directly, so we need to use a workaround
        var trends = await query
            .GroupBy(t => new { Year = t.CreatedAt.Year, Month = t.CreatedAt.Month, Day = t.CreatedAt.Day })
            .Select(g => new TicketTrendDto
            {
                Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                Opened = g.Count(),
                Resolved = g.Count(t => t.ResolvedAt.HasValue && 
                                       t.ResolvedAt.Value.Year == g.Key.Year &&
                                       t.ResolvedAt.Value.Month == g.Key.Month &&
                                       t.ResolvedAt.Value.Day == g.Key.Day),
                Closed = g.Count(t => t.ClosedAt.HasValue &&
                                     t.ClosedAt.Value.Year == g.Key.Year &&
                                     t.ClosedAt.Value.Month == g.Key.Month &&
                                     t.ClosedAt.Value.Day == g.Key.Day)
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        return trends;
    }
}
