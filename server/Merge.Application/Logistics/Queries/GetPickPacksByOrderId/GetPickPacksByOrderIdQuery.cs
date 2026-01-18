using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetPickPacksByOrderId;

public record GetPickPacksByOrderIdQuery(Guid OrderId) : IRequest<IEnumerable<PickPackDto>>;

