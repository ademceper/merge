using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.ValueObjects;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.SubscribeEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SubscribeEmailCommandHandler : IRequestHandler<SubscribeEmailCommand, EmailSubscriberDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscribeEmailCommandHandler> _logger;

    public SubscribeEmailCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SubscribeEmailCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailSubscriberDto> Handle(SubscribeEmailCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email aboneliği oluşturuluyor. Email: {Email}, Source: {Source}",
            request.Email, request.Source);

        // ✅ PERFORMANCE: Check if subscriber already exists (N+1 fix)
        var existing = await _context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.Restore();
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Subscribe();
            existing.UpdateDetails(
                firstName: request.FirstName,
                lastName: request.LastName,
                source: request.Source,
                tags: request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                customFields: request.CustomFields != null ? JsonSerializer.Serialize(request.CustomFields) : null);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload in one query (N+1 fix)
            // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
            var reloadedExisting = await _context.Set<EmailSubscriber>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == existing.Id, cancellationToken);

            // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
            _logger.LogInformation(
                "Email aboneliği güncellendi (mevcut kullanıcı). SubscriberId: {SubscriberId}, Email: {Email}",
                existing.Id, request.Email);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<EmailSubscriberDto>(reloadedExisting!);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // ✅ BOLUM 1.3: Value Objects - Email Value Object kullanımı
        var emailValue = new Email(request.Email);
        var subscriber = EmailSubscriber.Create(
            email: emailValue,
            firstName: request.FirstName,
            lastName: request.LastName,
            source: request.Source,
            tags: request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
            customFields: request.CustomFields != null ? JsonSerializer.Serialize(request.CustomFields) : null);

        await _context.Set<EmailSubscriber>().AddAsync(subscriber, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var createdSubscriber = await _context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == subscriber.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email aboneliği oluşturuldu. SubscriberId: {SubscriberId}, Email: {Email}",
            subscriber.Id, request.Email);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailSubscriberDto>(createdSubscriber!);
    }
}
