using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using System.Text.Json;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UpdateEmailSubscriber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateEmailSubscriberCommandHandler : IRequestHandler<UpdateEmailSubscriberCommand, EmailSubscriberDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmailSubscriberCommandHandler> _logger;

    public UpdateEmailSubscriberCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateEmailSubscriberCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EmailSubscriberDto> Handle(UpdateEmailSubscriberCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var subscriber = await _context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscriber == null)
        {
            throw new Merge.Application.Exceptions.NotFoundException("Email abonesi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        subscriber.UpdateDetails(
            firstName: request.FirstName,
            lastName: request.LastName,
            source: request.Source,
            tags: request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
            customFields: request.CustomFields != null ? JsonSerializer.Serialize(request.CustomFields) : null);

        if (request.IsSubscribed.HasValue)
        {
            if (request.IsSubscribed.Value)
                subscriber.Subscribe();
            else
                subscriber.Unsubscribe();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var updatedSubscriber = await _context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Email abonesi güncellendi. SubscriberId: {SubscriberId}",
            request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<EmailSubscriberDto>(updatedSubscriber!);
    }
}
