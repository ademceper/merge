using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteFlashSale;

public class DeleteFlashSaleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteFlashSaleCommandHandler> logger) : IRequestHandler<DeleteFlashSaleCommand, bool>
{
    public async Task<bool> Handle(DeleteFlashSaleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting flash sale. FlashSaleId: {FlashSaleId}", request.Id);

        var flashSale = await context.Set<FlashSale>()
            .FirstOrDefaultAsync(fs => fs.Id == request.Id, cancellationToken);

        if (flashSale is null)
        {
            logger.LogWarning("FlashSale not found. FlashSaleId: {FlashSaleId}", request.Id);
            throw new NotFoundException("Flash Sale", request.Id);
        }

        flashSale.MarkAsDeleted();

        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("FlashSale deleted successfully. FlashSaleId: {FlashSaleId}", request.Id);

        return true;
    }
}
