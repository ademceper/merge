using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetPickPackByPackNumber;

public record GetPickPackByPackNumberQuery(string PackNumber) : IRequest<PickPackDto?>;

