using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.UpdateFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateFaqCommandHandler : IRequestHandler<UpdateFaqCommand, FaqDto?>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFaqCommandHandler> _logger;

    public UpdateFaqCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateFaqCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FaqDto?> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating FAQ {FaqId}", request.FaqId);

        var faq = await _context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq == null)
        {
            _logger.LogWarning("FAQ {FaqId} not found for update", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        faq.Update(request.Question, request.Answer);
        faq.UpdateCategory(request.Category);
        faq.UpdateSortOrder(request.SortOrder);
        faq.SetPublished(request.IsPublished);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("FAQ {FaqId} updated successfully", request.FaqId);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<FaqDto>(faq);
    }
}
