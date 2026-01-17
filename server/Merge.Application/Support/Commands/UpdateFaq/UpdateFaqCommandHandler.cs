using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.UpdateFaq;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateFaqCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateFaqCommandHandler> logger) : IRequestHandler<UpdateFaqCommand, FaqDto?>
{

    public async Task<FaqDto?> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating FAQ {FaqId}", request.FaqId);

        var faq = await context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq == null)
        {
            logger.LogWarning("FAQ {FaqId} not found for update", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        faq.Update(request.Question, request.Answer);
        faq.UpdateCategory(request.Category);
        faq.UpdateSortOrder(request.SortOrder);
        faq.SetPublished(request.IsPublished);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("FAQ {FaqId} updated successfully", request.FaqId);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return mapper.Map<FaqDto>(faq);
    }
}
