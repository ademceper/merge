using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

public class DeleteCouponCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCouponCommandHandler> logger) : IRequestHandler<DeleteCouponCommand, bool>
{
    public async Task<bool> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting coupon. CouponId: {CouponId}", request.Id);

        var coupon = await context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon == null)
        {
            logger.LogWarning("Coupon not found. CouponId: {CouponId}", request.Id);
            throw new NotFoundException("Kupon", request.Id);
        }

        coupon.MarkAsDeleted();

        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Coupon deleted successfully. CouponId: {CouponId}", request.Id);

        return true;
    }
}
