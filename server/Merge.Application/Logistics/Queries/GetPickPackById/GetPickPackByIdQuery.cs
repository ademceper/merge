using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetPickPackById;

public record GetPickPackByIdQuery(Guid Id) : IRequest<PickPackDto?>;

