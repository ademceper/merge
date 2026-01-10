using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Commands.UpdateWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateWarehouseCommand(
    Guid Id,
    string Name,
    string Address,
    string City,
    string Country,
    string PostalCode,
    string ContactPerson,
    string ContactPhone,
    string ContactEmail,
    int Capacity,
    bool IsActive,
    string? Description) : IRequest<WarehouseDto>;

