using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

// Bu command sadece details (notes, weight, dimensions, packageCount) update için kullanılır
public record UpdatePickPackDetailsCommand(
    Guid PickPackId,
    string? Notes,
    decimal? Weight,
    string? Dimensions,
    int? PackageCount) : IRequest<Unit>;

