using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IDeliveryTimeEstimationService
{
    Task<DeliveryTimeEstimationDto> CreateEstimationAsync(CreateDeliveryTimeEstimationDto dto);
    Task<DeliveryTimeEstimationDto?> GetEstimationByIdAsync(Guid id);
    Task<IEnumerable<DeliveryTimeEstimationDto>> GetAllEstimationsAsync(Guid? productId = null, Guid? categoryId = null, Guid? warehouseId = null, bool? isActive = null);
    Task<bool> UpdateEstimationAsync(Guid id, UpdateDeliveryTimeEstimationDto dto);
    Task<bool> DeleteEstimationAsync(Guid id);
    Task<DeliveryTimeEstimateResultDto> EstimateDeliveryTimeAsync(EstimateDeliveryTimeDto dto);
}

