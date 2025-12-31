using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
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
    private readonly IEmailService _emailService;
    private readonly ILogger<SupportTicketService> _logger;

    public SupportTicketService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<SupportTicketService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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

        return await MapToDto(ticket);
    }

    public async Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, Guid? userId = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .Include(t => t.Attachments.Where(a => !a.IsDeleted))
            .Where(t => t.Id == ticketId);

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        var ticket = await query.FirstOrDefaultAsync();

        return ticket != null ? await MapToDto(ticket) : null;
    }

    public async Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null)
    {
        var query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages.Where(m => !m.IsDeleted))
                .ThenInclude(m => m.User)
            .Include(t => t.Attachments.Where(a => !a.IsDeleted))
            .Where(t => t.TicketNumber == ticketNumber);

        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }

        var ticket = await query.FirstOrDefaultAsync();

        return ticket != null ? await MapToDto(ticket) : null;
    }

    public async Task<IEnumerable<SupportTicketDto>> GetUserTicketsAsync(Guid userId, string? status = null)
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

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            dtos.Add(await MapToDto(ticket));
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

        var tickets = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            dtos.Add(await MapToDto(ticket));
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
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        return new TicketMessageDto
        {
            Id = message.Id,
            TicketId = message.TicketId,
            UserId = message.UserId,
            UserName = $"{message.User.FirstName} {message.User.LastName}",
            Message = message.Message,
            IsStaffResponse = message.IsStaffResponse,
            IsInternal = message.IsInternal,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(Guid ticketId, bool includeInternal = false)
    {
        var query = _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Attachments.Where(a => !a.IsDeleted))
            .Where(m => m.TicketId == ticketId);

        if (!includeInternal)
        {
            query = query.Where(m => !m.IsInternal);
        }

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(m => new TicketMessageDto
        {
            Id = m.Id,
            TicketId = m.TicketId,
            UserId = m.UserId,
            UserName = $"{m.User.FirstName} {m.User.LastName}",
            Message = m.Message,
            IsStaffResponse = m.IsStaffResponse,
            IsInternal = m.IsInternal,
            CreatedAt = m.CreatedAt,
            Attachments = m.Attachments.Select(a => new TicketAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                FileType = a.FileType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            }).ToList()
        }).ToList();
    }

    public async Task<TicketStatsDto> GetTicketStatsAsync()
    {
        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .ToListAsync();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddMonths(-1);

        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue).ToList();
        var avgResponseTime = resolvedTickets.Any() && resolvedTickets.Any(t => t.LastResponseAt.HasValue)
            ? resolvedTickets.Where(t => t.LastResponseAt.HasValue)
                .Average(t => (t.LastResponseAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var avgResolutionTime = resolvedTickets.Any()
            ? resolvedTickets.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        _logger.LogInformation("Ticket stats generated: Total {Total}, Resolved {Resolved}, Avg Resolution Time {AvgTime}h",
            tickets.Count, resolvedTickets.Count, Math.Round(avgResolutionTime, 2));

        return new TicketStatsDto
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
            ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
            ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
            TicketsToday = tickets.Count(t => t.CreatedAt >= today),
            TicketsThisWeek = tickets.Count(t => t.CreatedAt >= weekAgo),
            TicketsThisMonth = tickets.Count(t => t.CreatedAt >= monthAgo),
            AverageResponseTime = (decimal)Math.Round(avgResponseTime, 2),
            AverageResolutionTime = (decimal)Math.Round(avgResolutionTime, 2),
            TicketsByCategory = tickets.GroupBy(t => t.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TicketsByPriority = tickets.GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<SupportTicketDto>> GetUnassignedTicketsAsync()
    {
        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            dtos.Add(await MapToDto(ticket));
        }

        return dtos;
    }

    public async Task<IEnumerable<SupportTicketDto>> GetMyAssignedTicketsAsync(Guid agentId)
    {
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

        var dtos = new List<SupportTicketDto>();
        foreach (var ticket in tickets)
        {
            dtos.Add(await MapToDto(ticket));
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

    private async Task<SupportTicketDto> MapToDto(SupportTicket ticket)
    {
        var user = ticket.User ?? await _context.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == ticket.UserId);

        var order = ticket.Order ?? (ticket.OrderId.HasValue
            ? await _context.Set<OrderEntity>().AsNoTracking().FirstOrDefaultAsync(o => o.Id == ticket.OrderId.Value)
            : null);

        var product = ticket.Product ?? (ticket.ProductId.HasValue
            ? await _context.Set<ProductEntity>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == ticket.ProductId.Value)
            : null);

        var assignedTo = ticket.AssignedTo ?? (ticket.AssignedToId.HasValue
            ? await _context.Set<UserEntity>().AsNoTracking().FirstOrDefaultAsync(u => u.Id == ticket.AssignedToId.Value)
            : null);

        var messages = ticket.Messages?.Where(m => !m.IsDeleted).Select(m => new TicketMessageDto
        {
            Id = m.Id,
            TicketId = m.TicketId,
            UserId = m.UserId,
            UserName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "Unknown",
            Message = m.Message,
            IsStaffResponse = m.IsStaffResponse,
            IsInternal = m.IsInternal,
            CreatedAt = m.CreatedAt
        }).ToList() ?? new List<TicketMessageDto>();

        var attachments = ticket.Attachments?.Where(a => !a.IsDeleted).Select(a => new TicketAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FilePath = a.FilePath,
            FileType = a.FileType,
            FileSize = a.FileSize,
            CreatedAt = a.CreatedAt
        }).ToList() ?? new List<TicketAttachmentDto>();

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId = ticket.UserId,
            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            UserEmail = user?.Email ?? string.Empty,
            Category = ticket.Category.ToString(),
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            Subject = ticket.Subject,
            Description = ticket.Description,
            OrderId = ticket.OrderId,
            OrderNumber = order?.OrderNumber,
            ProductId = ticket.ProductId,
            ProductName = product?.Name,
            AssignedToId = ticket.AssignedToId,
            AssignedToName = assignedTo != null ? $"{assignedTo.FirstName} {assignedTo.LastName}" : null,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,
            ResponseCount = ticket.ResponseCount,
            LastResponseAt = ticket.LastResponseAt,
            CreatedAt = ticket.CreatedAt,
            Messages = messages,
            Attachments = attachments
        };
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

        // Get all tickets assigned to this agent
        var allTickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Messages)
            .Where(t => t.AssignedToId == agentId)
            .ToListAsync();

        var periodTickets = allTickets
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToList();

        // Overview stats
        var totalTickets = allTickets.Count;
        var openTickets = allTickets.Count(t => t.Status == TicketStatus.Open);
        var inProgressTickets = allTickets.Count(t => t.Status == TicketStatus.InProgress);
        var resolvedTickets = allTickets.Count(t => t.Status == TicketStatus.Resolved);
        var closedTickets = allTickets.Count(t => t.Status == TicketStatus.Closed);

        // Unassigned tickets (for admin view)
        var unassignedTickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .CountAsync(t => t.AssignedToId == null && t.Status != TicketStatus.Closed);

        // Performance metrics
        var resolvedTicketsWithTime = allTickets
            .Where(t => t.ResolvedAt.HasValue)
            .ToList();

        var averageResolutionTime = resolvedTicketsWithTime.Any()
            ? resolvedTicketsWithTime.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var ticketsWithResponse = allTickets
            .Where(t => t.LastResponseAt.HasValue)
            .ToList();

        var averageResponseTime = ticketsWithResponse.Any()
            ? ticketsWithResponse.Average(t => (t.LastResponseAt!.Value - t.CreatedAt).TotalHours)
            : 0;

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var ticketsResolvedToday = allTickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today);
        var ticketsResolvedThisWeek = allTickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= weekAgo);
        var ticketsResolvedThisMonth = allTickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= monthAgo);

        var resolutionRate = totalTickets > 0
            ? (decimal)(resolvedTickets + closedTickets) / totalTickets * 100
            : 0;

        // Workload metrics
        var activeTickets = allTickets.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);
        var overdueTickets = allTickets.Count(t =>
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
            t.CreatedAt < DateTime.UtcNow.AddDays(-3)); // Tickets older than 3 days
        var highPriorityTickets = allTickets.Count(t => t.Priority == TicketPriority.High);
        var urgentTickets = allTickets.Count(t => t.Priority == TicketPriority.Urgent);

        // Category breakdown
        var ticketsByCategory = await GetTicketsByCategoryAsync(agentId, startDate, endDate);

        // Priority breakdown
        var ticketsByPriority = await GetTicketsByPriorityAsync(agentId, startDate, endDate);

        // Trends
        var trends = await GetTicketTrendsAsync(agentId, startDate, endDate);

        // Recent tickets
        var recentTickets = allTickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToList();

        var recentTicketsDto = new List<SupportTicketDto>();
        foreach (var ticket in recentTickets)
        {
            recentTicketsDto.Add(await MapToDto(ticket));
        }

        // Urgent tickets
        var urgentTicketsList = allTickets
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(10)
            .ToList();

        var urgentTicketsDto = new List<SupportTicketDto>();
        foreach (var ticket in urgentTicketsList)
        {
            urgentTicketsDto.Add(await MapToDto(ticket));
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

        var tickets = await query.ToListAsync();
        var total = tickets.Count;

        return tickets
            .GroupBy(t => t.Category.ToString())
            .Select(g => new CategoryTicketCountDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToList();
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

        var tickets = await query.ToListAsync();
        var total = tickets.Count;

        return tickets
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityTicketCountDto
            {
                Priority = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(p => p.Count)
            .ToList();
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

        var tickets = await query.ToListAsync();

        return tickets
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new TicketTrendDto
            {
                Date = g.Key,
                Opened = g.Count(),
                Resolved = g.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == g.Key),
                Closed = g.Count(t => t.ClosedAt.HasValue && t.ClosedAt.Value.Date == g.Key)
            })
            .OrderBy(t => t.Date)
            .ToList();
    }
}
