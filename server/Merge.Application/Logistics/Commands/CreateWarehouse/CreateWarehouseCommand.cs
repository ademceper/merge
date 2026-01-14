using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Logistics.Commands.CreateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateWarehouseCommand(
    string Name,
    string Code,
    string Address,
    string City,
    string Country,
    string PostalCode,
    string ContactPerson,
    string ContactPhone,
    string ContactEmail,
    int Capacity,
    string? Description) : IRequest<WarehouseDto>;

