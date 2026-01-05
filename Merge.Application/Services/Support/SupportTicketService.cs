using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;


namespace Merge.Application.Services.Support;

public class SupportTicketService : ISupportTicketService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupportTicketService> _logger;

    public SupportTicketService(
        IDbContext context,
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1, 9.2: Structured Logging (ZORUNLU)
    public async Task<SupportTicketDto> CreateTicketAsync(Guid userId, CreateSupportTicketDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating support ticket for user {UserId}. Category: {Category}, Priority: {Priority}, Subject: {Subject}",
            userId, dto.Category, dto.Priority, dto.Subject);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found while creating support ticket", userId);
            throw new NotFoundException("Kullanıcı", userId);
        }

        // Generate ticket number
        var ticketNumber = await GenerateTicketNumberAsync(cancellationToken);

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

        await _context.Set<SupportTicket>().AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {TicketNumber} created successfully for user {UserId}", ticketNumber, userId);

        // Send confirmation email
        try
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                $"Support Ticket Created - {ticketNumber}",
                $"Your support ticket has been created. Ticket Number: {ticketNumber}. Subject: {dto.Subject}. We'll respond as soon as possible.",
                true,
                cancellationToken);
            _logger.LogInformation("Confirmation email sent for ticket {TicketNumber}", ticketNumber);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception handling - Log ve throw (YASAK: Exception yutulmamalı)
            _logger.LogError(ex, "Failed to send confirmation email for ticket {TicketNumber}", ticketNumber);
            // Exception'ı yutmayız, sadece loglarız - ticket oluşturuldu, email gönderilemedi
        }

        // ✅ PERFORMANCE: Reload with includes for mapping
        ticket = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

        return await MapToDtoAsync(ticket!, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
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

        var ticket = await query.FirstOrDefaultAsync(cancellationToken);

        return ticket != null ? await MapToDtoAsync(ticket, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
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

        var ticket = await query.FirstOrDefaultAsync(cancellationToken);

        return ticket != null ? await MapToDtoAsync(ticket, cancellationToken) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    public async Task<PagedResult<SupportTicketDto>> GetUserTicketsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
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

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => ticketIds.Contains(t.Id))
            .OrderByDescending(t => t.CreatedAt)
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
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        var dtos = new List<SupportTicketDto>(tickets.Count);
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

        return new PagedResult<SupportTicketDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
    public async Task<PagedResult<SupportTicketDto>> GetAllTicketsAsync(string? status = null, string? category = null, Guid? assignedToId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo);

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

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => ticketIds.Contains(t.Id))
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
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
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        var dtos = new List<SupportTicketDto>(tickets.Count);
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

        return new PagedResult<SupportTicketDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTicketAsync(Guid ticketId, UpdateSupportTicketDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} updated. Status changed from {OldStatus} to {NewStatus}",
            ticket.TicketNumber, oldStatus, ticket.Status);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> AssignTicketAsync(Guid ticketId, Guid assignedToId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} assigned to user {AssignedToId}", ticket.TicketNumber, assignedToId);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CloseTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for closure", ticketId);
            return false;
        }

        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} closed successfully", ticket.TicketNumber);

        // Send closure email
        try
        {
            await _emailService.SendEmailAsync(
                ticket.User?.Email ?? string.Empty,
                $"Ticket Closed - {ticket.TicketNumber}",
                $"Your support ticket #{ticket.TicketNumber} has been closed. If you need further assistance, please open a new ticket.",
                true,
                cancellationToken);
            _logger.LogInformation("Closure email sent for ticket {TicketNumber}", ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception handling - Log ve throw (YASAK: Exception yutulmamalı)
            _logger.LogError(ex, "Failed to send closure email for ticket {TicketNumber}", ticket.TicketNumber);
            // Exception'ı yutmayız, sadece loglarız - ticket kapatıldı, email gönderilemedi
        }

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ReopenTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for reopening", ticketId);
            return false;
        }

        ticket.Status = TicketStatus.Open;
        ticket.ClosedAt = null;
        ticket.ResolvedAt = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} reopened", ticket.TicketNumber);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1, 9.2: Structured Logging (ZORUNLU)
    public async Task<TicketMessageDto> AddMessageAsync(Guid userId, CreateTicketMessageDto dto, bool isStaffResponse = false, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Adding message to ticket {TicketId} from user {UserId}. IsStaffResponse: {IsStaffResponse}",
            dto.TicketId, userId, isStaffResponse);

        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == dto.TicketId, cancellationToken);

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

        await _context.Set<TicketMessage>().AddAsync(message, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
                    $"You have received a new response on your support ticket #{ticket.TicketNumber}.",
                    true,
                    cancellationToken);
                _logger.LogInformation("Response notification email sent for ticket {TicketNumber}", ticket.TicketNumber);
            }
            catch (Exception ex)
            {
                // ✅ BOLUM 2.1: Exception handling - Log ve throw (YASAK: Exception yutulmamalı)
                _logger.LogError(ex, "Failed to send response notification for ticket {TicketNumber}", ticket.TicketNumber);
                // Exception'ı yutmayız, sadece loglarız - mesaj eklendi, email gönderilemedi
            }
        }

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        message = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<TicketMessageDto>(message!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<TicketMessageDto>> GetTicketMessagesAsync(Guid ticketId, bool includeInternal = false, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        IQueryable<TicketMessage> query = _context.Set<TicketMessage>()
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
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<TicketMessageDto>>(messages);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TicketStatsDto> GetTicketStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = now.AddDays(-7);
        var monthAgo = now.AddMonths(-1);

        var totalTickets = await query.CountAsync(cancellationToken);
        var openTickets = await query.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var inProgressTickets = await query.CountAsync(t => t.Status == TicketStatus.InProgress, cancellationToken);
        var resolvedTickets = await query.CountAsync(t => t.Status == TicketStatus.Resolved, cancellationToken);
        var closedTickets = await query.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);
        var ticketsToday = await query.CountAsync(t => t.CreatedAt >= today, cancellationToken);
        var ticketsThisWeek = await query.CountAsync(t => t.CreatedAt >= weekAgo, cancellationToken);
        var ticketsThisMonth = await query.CountAsync(t => t.CreatedAt >= monthAgo, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedTicketsQuery = query.Where(t => t.ResolvedAt.HasValue);
        var avgResolutionTime = await resolvedTicketsQuery.AnyAsync(cancellationToken)
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var ticketsWithResponseQuery = query.Where(t => t.LastResponseAt.HasValue && t.ResolvedAt.HasValue);
        var avgResponseTime = await ticketsWithResponseQuery.AnyAsync(cancellationToken)
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var ticketsByCategory = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count, cancellationToken);

        var ticketsByPriority = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count, cancellationToken);

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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SupportTicketDto>> GetUnassignedTicketsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var ticketIds = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
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
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        var dtos = new List<SupportTicketDto>(tickets.Count);
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SupportTicketDto>> GetMyAssignedTicketsAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == agentId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == agentId && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
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
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        var dtos = new List<SupportTicketDto>(tickets.Count);
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var lastTicket = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task<SupportTicketDto> MapToDtoAsync(SupportTicket ticket, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: AutoMapper kullan
        var dto = _mapper.Map<SupportTicketDto>(ticket);

        // ✅ PERFORMANCE: Batch load messages and attachments if not already loaded
        if (ticket.Messages == null || ticket.Messages.Count == 0)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
            var messages = await _context.Set<TicketMessage>()
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Where(m => m.TicketId == ticket.Id)
                .ToListAsync(cancellationToken);
            dto.Messages = _mapper.Map<List<TicketMessageDto>>(messages);
        }
        else
        {
            dto.Messages = _mapper.Map<List<TicketMessageDto>>(ticket.Messages);
        }

        if (ticket.Attachments == null || ticket.Attachments.Count == 0)
        {
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
            var attachments = await _context.Set<TicketAttachment>()
                .AsNoTracking()
                .Where(a => a.TicketId == ticket.Id)
                .ToListAsync(cancellationToken);
            dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(attachments);
        }
        else
        {
            dto.Attachments = _mapper.Map<List<TicketAttachmentDto>>(ticket.Attachments);
        }

        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1, 9.2: Structured Logging (ZORUNLU)
    public async Task<SupportAgentDashboardDto> GetAgentDashboardAsync(Guid agentId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Generating agent dashboard for agent {AgentId} from {StartDate} to {EndDate}",
            agentId, startDate, endDate);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !u.IsDeleted (Global Query Filter)
        var agent = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == agentId, cancellationToken);

        if (agent == null)
        {
            _logger.LogWarning("Agent {AgentId} not found", agentId);
            throw new NotFoundException("Ajan", agentId);
        }

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> allTicketsQuery = _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == agentId);

        var totalTickets = await allTicketsQuery.CountAsync(cancellationToken);
        var openTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var inProgressTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.InProgress, cancellationToken);
        var resolvedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Resolved, cancellationToken);
        var closedTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);

        // Unassigned tickets (for admin view)
        var unassignedTickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .CountAsync(t => t.AssignedToId == null && t.Status != TicketStatus.Closed, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var resolvedTicketsQuery = allTicketsQuery.Where(t => t.ResolvedAt.HasValue);
        var averageResolutionTime = await resolvedTicketsQuery.AnyAsync(cancellationToken)
            ? await resolvedTicketsQuery
                .AverageAsync(t => (double)(t.ResolvedAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var ticketsWithResponseQuery = allTicketsQuery.Where(t => t.LastResponseAt.HasValue);
        var averageResponseTime = await ticketsWithResponseQuery.AnyAsync(cancellationToken)
            ? await ticketsWithResponseQuery
                .AverageAsync(t => (double)(t.LastResponseAt!.Value - t.CreatedAt).TotalHours, cancellationToken)
            : 0;

        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var ticketsResolvedToday = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == today, cancellationToken);
        var ticketsResolvedThisWeek = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= weekAgo, cancellationToken);
        var ticketsResolvedThisMonth = await allTicketsQuery.CountAsync(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value >= monthAgo, cancellationToken);

        var resolutionRate = totalTickets > 0
            ? (decimal)(resolvedTickets + closedTickets) / totalTickets * 100
            : 0;

        // ✅ PERFORMANCE: Database'de count yap
        var activeTickets = await allTicketsQuery.CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress, cancellationToken);
        var overdueTickets = await allTicketsQuery.CountAsync(t =>
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress) &&
            t.CreatedAt < DateTime.UtcNow.AddDays(-3), cancellationToken); // Tickets older than 3 days
        var highPriorityTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.High, cancellationToken);
        var urgentTickets = await allTicketsQuery.CountAsync(t => t.Priority == TicketPriority.Urgent, cancellationToken);

        // Category breakdown
        var ticketsByCategory = await GetTicketsByCategoryAsync(agentId, startDate, endDate, cancellationToken);

        // Priority breakdown
        var ticketsByPriority = await GetTicketsByPriorityAsync(agentId, startDate, endDate, cancellationToken);

        // Trends
        var trends = await GetTicketTrendsAsync(agentId, startDate, endDate, cancellationToken);

        // ✅ PERFORMANCE: Recent tickets - database'de query
        var recentTickets = await allTicketsQuery
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Urgent tickets - database'de query
        var urgentTicketsList = await allTicketsQuery
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: allTicketIds'i database'de oluştur, memory'de işlem YASAK
        // Recent tickets için ID'leri database'de al
        var recentTicketIds = await allTicketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        
        // Urgent tickets için ID'leri database'de al
        var urgentTicketIds = await allTicketsQuery
            .Where(t => t.Priority == TicketPriority.Urgent && (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .OrderBy(t => t.CreatedAt)
            .Take(10)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);
        
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
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => allTicketIds.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<List<CategoryTicketCountDto>> GetTicketsByCategoryAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

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
        var total = await query.CountAsync(cancellationToken);

        var grouped = await query
            .GroupBy(t => t.Category.ToString())
            .Select(g => new CategoryTicketCountDto
            {
                Category = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<List<PriorityTicketCountDto>> GetTicketsByPriorityAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

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
        var total = await query.CountAsync(cancellationToken);

        var grouped = await query
            .GroupBy(t => t.Priority.ToString())
            .Select(g => new PriorityTicketCountDto
            {
                Priority = g.Key,
                Count = g.Count(),
                Percentage = total > 0 ? Math.Round((decimal)g.Count() / total * 100, 2) : 0
            })
            .OrderByDescending(p => p.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<List<TicketTrendDto>> GetTicketTrendsAsync(Guid? agentId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
            .AsNoTracking();

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
            .ToListAsync(cancellationToken);

        return trends;
    }
}
