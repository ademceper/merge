using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

public class DeleteKnowledgeBaseCategoryCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteKnowledgeBaseCategoryCommandHandler> logger) : IRequestHandler<DeleteKnowledgeBaseCategoryCommand, bool>
{

    public async Task<bool> Handle(DeleteKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting knowledge base category {CategoryId}", request.CategoryId);

        var category = await context.Set<KnowledgeBaseCategory>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
        {
            logger.LogWarning("Knowledge base category {CategoryId} not found for deletion", request.CategoryId);
            throw new NotFoundException("Bilgi bankası kategorisi", request.CategoryId);
        }

        category.MarkAsDeleted();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Knowledge base category {CategoryId} deleted successfully", request.CategoryId);

        return true;
    }
}
