using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Seller;

public interface ISellerOnboardingService
{
    Task<SellerApplicationDto> SubmitApplicationAsync(Guid userId, CreateSellerApplicationDto applicationDto, CancellationToken cancellationToken = default);
    Task<SellerApplicationDto?> GetApplicationByIdAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<SellerApplicationDto?> GetUserApplicationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<SellerApplicationDto>> GetAllApplicationsAsync(SellerApplicationStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<SellerApplicationDto> ReviewApplicationAsync(Guid applicationId, ReviewSellerApplicationDto reviewDto, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<bool> ApproveApplicationAsync(Guid applicationId, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<bool> RejectApplicationAsync(Guid applicationId, string reason, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<SellerOnboardingStatsDto> GetOnboardingStatsAsync(CancellationToken cancellationToken = default);
}
