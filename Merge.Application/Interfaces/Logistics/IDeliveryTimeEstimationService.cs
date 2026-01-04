using Merge.Application.DTOs.Logistics;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Logistics;

public interface IDeliveryTimeEstimationService
{
    Task<DeliveryTimeEstimationDto> CreateEstimationAsync(CreateDeliveryTimeEstimationDto dto, CancellationToken cancellationToken = default);
    Task<DeliveryTimeEstimationDto?> GetEstimationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeliveryTimeEstimationDto>> GetAllEstimationsAsync(Guid? productId = null, Guid? categoryId = null, Guid? warehouseId = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateEstimationAsync(Guid id, UpdateDeliveryTimeEstimationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteEstimationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeliveryTimeEstimateResultDto> EstimateDeliveryTimeAsync(EstimateDeliveryTimeDto dto, CancellationToken cancellationToken = default);
}

