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

public class UpdateFaqCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateFaqCommandHandler> logger) : IRequestHandler<UpdateFaqCommand, FaqDto?>
{

    public async Task<FaqDto?> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating FAQ {FaqId}", request.FaqId);

        var faq = await context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq is null)
        {
            logger.LogWarning("FAQ {FaqId} not found for update", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        faq.Update(request.Question, request.Answer);
        faq.UpdateCategory(request.Category);
        faq.UpdateSortOrder(request.SortOrder);
        faq.SetPublished(request.IsPublished);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("FAQ {FaqId} updated successfully", request.FaqId);

        return mapper.Map<FaqDto>(faq);
    }
}
