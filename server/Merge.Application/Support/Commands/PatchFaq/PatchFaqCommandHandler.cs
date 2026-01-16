using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.PatchFaq;

/// <summary>
/// Handler for PatchFaqCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchFaqCommandHandler : IRequestHandler<PatchFaqCommand, FaqDto?>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PatchFaqCommandHandler> _logger;

    public PatchFaqCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PatchFaqCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FaqDto?> Handle(PatchFaqCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Patching FAQ {FaqId}", request.FaqId);

        var faq = await _context.Set<FAQ>()
            .FirstOrDefaultAsync(f => f.Id == request.FaqId, cancellationToken);

        if (faq == null)
        {
            _logger.LogWarning("FAQ {FaqId} not found for patch", request.FaqId);
            throw new NotFoundException("FAQ", request.FaqId);
        }

        // Apply partial updates - only update fields that are provided
        if (request.PatchDto.Question != null || request.PatchDto.Answer != null)
        {
            var question = request.PatchDto.Question ?? faq.Question;
            var answer = request.PatchDto.Answer ?? faq.Answer;
            faq.Update(question, answer);
        }

        if (request.PatchDto.Category != null)
        {
            faq.UpdateCategory(request.PatchDto.Category);
        }

        if (request.PatchDto.SortOrder.HasValue)
        {
            faq.UpdateSortOrder(request.PatchDto.SortOrder.Value);
        }

        if (request.PatchDto.IsPublished.HasValue)
        {
            faq.SetPublished(request.PatchDto.IsPublished.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("FAQ {FaqId} patched successfully", request.FaqId);

        return _mapper.Map<FaqDto>(faq);
    }
}
