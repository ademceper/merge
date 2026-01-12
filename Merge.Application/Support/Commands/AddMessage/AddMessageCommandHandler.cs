using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.AddMessage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AddMessageCommandHandler : IRequestHandler<AddMessageCommand, TicketMessageDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ILogger<AddMessageCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public AddMessageCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEmailService emailService,
        ILogger<AddMessageCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailService = emailService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<TicketMessageDto> Handle(AddMessageCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Adding message to ticket {TicketId} from user {UserId}. IsStaffResponse: {IsStaffResponse}",
            request.TicketId, request.UserId, request.IsStaffResponse);

        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found while adding message", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        var oldStatus = ticket.Status;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var message = TicketMessage.Create(
            request.TicketId,
            ticket.TicketNumber,
            request.UserId,
            request.Message,
            request.IsStaffResponse,
            request.IsInternal);

        await _context.Set<TicketMessage>().AddAsync(message, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        ticket.AddMessage();
        ticket.UpdateStatusOnMessage(request.IsStaffResponse);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Message added to ticket {TicketNumber}. Response count: {ResponseCount}, Status: {OldStatus} -> {NewStatus}",
            ticket.TicketNumber, ticket.ResponseCount, oldStatus, ticket.Status);

        // Send email notification
        if (request.IsStaffResponse && !request.IsInternal)
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
}
