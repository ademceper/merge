using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CreateFaq;

public class CreateFaqCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateFaqCommandHandler> logger) : IRequestHandler<CreateFaqCommand, FaqDto>
{

    public async Task<FaqDto> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating FAQ. Category: {Category}, IsPublished: {IsPublished}",
            request.Category, request.IsPublished);

        var faq = FAQ.Create(
            request.Question,
            request.Answer,
            request.Category,
            request.SortOrder,
            request.IsPublished);

        await context.Set<FAQ>().AddAsync(faq, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("FAQ {FaqId} created successfully. Category: {Category}", faq.Id, request.Category);

        return mapper.Map<FaqDto>(faq);
    }
}
