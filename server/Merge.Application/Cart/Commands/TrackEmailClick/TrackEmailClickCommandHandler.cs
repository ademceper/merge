using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.TrackEmailClick;

public class TrackEmailClickCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<TrackEmailClickCommandHandler> logger) : IRequestHandler<TrackEmailClickCommand, bool>
{

    public async Task<bool> Handle(TrackEmailClickCommand request, CancellationToken cancellationToken)
    {
        var email = await context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == request.EmailId, cancellationToken);

        if (email is null)
        {
            return false;
        }

        email.MarkAsClicked();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}

