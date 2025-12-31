using Merge.Application.DTOs.Security;
namespace Merge.Application.Interfaces.Security;

public interface IOrderVerificationService
{
    Task<OrderVerificationDto> CreateVerificationAsync(CreateOrderVerificationDto dto);
    Task<OrderVerificationDto?> GetVerificationByOrderIdAsync(Guid orderId);
    Task<IEnumerable<OrderVerificationDto>> GetPendingVerificationsAsync();
    Task<bool> VerifyOrderAsync(Guid verificationId, Guid verifiedByUserId, string? notes = null);
    Task<bool> RejectOrderAsync(Guid verificationId, Guid verifiedByUserId, string reason);
    Task<IEnumerable<OrderVerificationDto>> GetAllVerificationsAsync(string? status = null, int page = 1, int pageSize = 20);
}


