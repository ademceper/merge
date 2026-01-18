using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.UnsubscribeEmail;

public class UnsubscribeEmailCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UnsubscribeEmailCommandHandler> logger) : IRequestHandler<UnsubscribeEmailCommand, bool>
{
    public async Task<bool> Handle(UnsubscribeEmailCommand request, CancellationToken cancellationToken)
    {
        var subscriber = await context.Set<EmailSubscriber>()
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Email, request.Email), cancellationToken);

        if (subscriber is null)
        {
            return false;
        }

        subscriber.Unsubscribe();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Email aboneliği iptal edildi. Email: {Email}",
            request.Email);

        return true;
    }
}
