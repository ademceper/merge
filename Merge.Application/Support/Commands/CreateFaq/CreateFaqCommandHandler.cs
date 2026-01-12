using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.CreateFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateFaqCommandHandler : IRequestHandler<CreateFaqCommand, FaqDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFaqCommandHandler> _logger;

    public CreateFaqCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateFaqCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FaqDto> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating FAQ. Category: {Category}, IsPublished: {IsPublished}",
            request.Category, request.IsPublished);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var faq = FAQ.Create(
            request.Question,
            request.Answer,
            request.Category,
            request.SortOrder,
            request.IsPublished);

        await _context.Set<FAQ>().AddAsync(faq, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("FAQ {FaqId} created successfully. Category: {Category}", faq.Id, request.Category);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<FaqDto>(faq);
    }
}
