using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Commands.DeleteCoupon;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCouponCommandHandler> _logger;

    public DeleteCouponCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteCouponCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting coupon. CouponId: {CouponId}", request.Id);

        var coupon = await _context.Set<Coupon>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon == null)
        {
            _logger.LogWarning("Coupon not found. CouponId: {CouponId}", request.Id);
            throw new NotFoundException("Kupon", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı (soft delete)
        // NOT: Coupon entity'de MarkAsDeleted metodu yok, BaseEntity'deki IsDeleted property'sini kullanıyoruz
        // Ancak domain logic için MarkAsDeleted metodu eklenmeli
        coupon.MarkAsDeleted();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Coupon deleted successfully. CouponId: {CouponId}", request.Id);

        return true;
    }
}
