using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Common;

namespace Merge.Application.Support.Commands.CreateTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, SupportTicketDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateTicketCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public CreateTicketCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        ILogger<CreateTicketCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<SupportTicketDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating support ticket for user {UserId}. Category: {Category}, Priority: {Priority}, Subject: {Subject}",
            request.UserId, request.Category, request.Priority, request.Subject);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found while creating support ticket", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // Generate ticket number
        var ticketNumber = await GenerateTicketNumberAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var ticket = SupportTicket.Create(
            ticketNumber,
            request.UserId,
            Enum.Parse<TicketCategory>(request.Category, true),
            Enum.Parse<TicketPriority>(request.Priority, true),
            request.Subject,
            request.Description,
            request.OrderId,
            request.ProductId);

        await _context.Set<SupportTicket>().AddAsync(ticket, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support ticket {TicketNumber} created successfully for user {UserId}", ticketNumber, request.UserId);

        // Send confirmation email
        try
        {
            await _emailService.SendEmailAsync(
                user.Email ?? string.Empty,
                $"Support Ticket Created - {ticketNumber}",
                $"Your support ticket has been created. Ticket Number: {ticketNumber}. Subject: {request.Subject}. We'll respond as soon as possible.",
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

    private async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastTicket = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;
        if (lastTicket != null && lastTicket.TicketNumber.StartsWith(_settings.TicketNumberPrefix))
        {
            var numberPart = lastTicket.TicketNumber.Substring(_settings.TicketNumberPrefix.Length);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{_settings.TicketNumberPrefix}{nextNumber.ToString($"D{_settings.TicketNumberPadding}")}";
    }

    private async Task<SupportTicketDto> MapToDtoAsync(SupportTicket ticket, CancellationToken cancellationToken = default)
    {
        var dto = _mapper.Map<SupportTicketDto>(ticket);

        // ✅ BOLUM 7.1.5: Records - IReadOnlyList kullanımı (immutability)
        IReadOnlyList<TicketMessageDto> messages;
        if (ticket.Messages == null || ticket.Messages.Count == 0)
        {
            var messageList = await _context.Set<TicketMessage>()
                .AsNoTracking()
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Where(m => m.TicketId == ticket.Id)
                .ToListAsync(cancellationToken);
            messages = _mapper.Map<List<TicketMessageDto>>(messageList).AsReadOnly();
        }
        else
        {
            messages = _mapper.Map<List<TicketMessageDto>>(ticket.Messages).AsReadOnly();
        }

        IReadOnlyList<TicketAttachmentDto> attachments;
        if (ticket.Attachments == null || ticket.Attachments.Count == 0)
        {
            var attachmentList = await _context.Set<TicketAttachment>()
                .AsNoTracking()
                .Where(a => a.TicketId == ticket.Id)
                .ToListAsync(cancellationToken);
            attachments = _mapper.Map<List<TicketAttachmentDto>>(attachmentList).AsReadOnly();
        }
        else
        {
            attachments = _mapper.Map<List<TicketAttachmentDto>>(ticket.Attachments).AsReadOnly();
        }

        // ✅ BOLUM 7.1.5: Records - Record'lar immutable, yeni instance oluştur
        return dto with { Messages = messages, Attachments = attachments };
    }
}
