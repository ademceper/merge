using Merge.Domain.Entities;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Interfaces.Seller;

public interface ISellerOnboardingService
{
    Task<SellerApplicationDto> SubmitApplicationAsync(Guid userId, CreateSellerApplicationDto applicationDto);
    Task<SellerApplicationDto?> GetApplicationByIdAsync(Guid applicationId);
    Task<SellerApplicationDto?> GetUserApplicationAsync(Guid userId);
    Task<IEnumerable<SellerApplicationDto>> GetAllApplicationsAsync(SellerApplicationStatus? status = null, int page = 1, int pageSize = 20);
    Task<SellerApplicationDto> ReviewApplicationAsync(Guid applicationId, ReviewSellerApplicationDto reviewDto, Guid reviewerId);
    Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId);
    Task<bool> RejectApplicationAsync(Guid applicationId, string reason, Guid reviewerId);
    Task<SellerOnboardingStatsDto> GetOnboardingStatsAsync();
}
