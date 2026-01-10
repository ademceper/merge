using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetPickPackByPackNumber;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPickPackByPackNumberQuery(string PackNumber) : IRequest<PickPackDto?>;

