using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.CreateLiveChatSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateLiveChatSessionCommandHandler : IRequestHandler<CreateLiveChatSessionCommand, LiveChatSessionDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLiveChatSessionCommandHandler> _logger;
    private readonly SupportSettings _settings;

    public CreateLiveChatSessionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateLiveChatSessionCommandHandler> logger,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<LiveChatSessionDto> Handle(CreateLiveChatSessionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating live chat session. UserId: {UserId}, GuestName: {GuestName}, Department: {Department}",
            request.UserId, request.GuestName, request.Department);

        var sessionId = GenerateSessionId();

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var session = LiveChatSession.Create(
            sessionId,
            request.UserId,
            request.GuestName,
            request.GuestEmail,
            request.Department,
            null, // IP address will be set by controller if needed
            null); // User agent will be set by controller if needed

        await _context.Set<LiveChatSession>().AddAsync(session, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Live chat session {SessionId} created successfully", sessionId);

        // ✅ PERFORMANCE: Reload with includes for mapping
        session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(_settings.MaxRecentChatMessages))
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveChatSessionDto>(session!);
    }

    private string GenerateSessionId()
    {
        var datePart = DateTime.UtcNow.ToString(_settings.ChatSessionIdDateFormat);
        var guidPart = Guid.NewGuid().ToString().Substring(0, _settings.ChatSessionIdGuidLength).ToUpper();
        return $"{_settings.ChatSessionIdPrefix}{datePart}-{guidPart}";
    }
}
