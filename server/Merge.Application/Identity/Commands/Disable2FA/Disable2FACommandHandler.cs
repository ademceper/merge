using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Exceptions;
using Merge.Application.Identity.Commands.Verify2FACode;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Identity.TwoFactorAuth>;

namespace Merge.Application.Identity.Commands.Disable2FA;

public class Disable2FACommandHandler(
    IRepository twoFactorRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<Disable2FACommandHandler> logger) : IRequestHandler<Disable2FACommand, Unit>
{

    public async Task<Unit> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Disabling 2FA. UserId: {UserId}", request.UserId);

        var twoFactorAuth = await context.Set<TwoFactorAuth>()
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (twoFactorAuth is null || !twoFactorAuth.IsEnabled)
        {
            logger.LogWarning("2FA disable failed - not enabled. UserId: {UserId}", request.UserId);
            throw new BusinessException("2FA etkin değil.");
        }

        var verifyCommand = new Verify2FACodeCommand(request.UserId, request.DisableDto.Code);
        var isValid = await mediator.Send(verifyCommand, cancellationToken);

        if (!isValid)
        {
            logger.LogWarning("2FA disable failed - invalid code. UserId: {UserId}", request.UserId);
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        twoFactorAuth.Disable();
        await twoFactorRepository.UpdateAsync(twoFactorAuth);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("2FA disabled successfully. UserId: {UserId}", request.UserId);
        return Unit.Value;
    }
}

