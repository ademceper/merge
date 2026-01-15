using AutoMapper;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using InventoryEntity = Merge.Domain.Modules.Inventory.Inventory;
using System.Text.Json;

namespace Merge.Application.Mappings.Inventory;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        // Warehouse mappings
        CreateMap<Warehouse, WarehouseDto>()
        .ConstructUsing(src => new WarehouseDto(
        src.Id,
        src.Name,
        src.Code,
        src.Address,
        src.City,
        src.Country,
        src.PostalCode,
        src.ContactPerson,
        src.ContactPhone,
        src.ContactEmail,
        src.Capacity,
        src.IsActive,
        src.Description,
        src.CreatedAt
        ));
        CreateMap<CreateWarehouseDto, Warehouse>();
        CreateMap<UpdateWarehouseDto, Warehouse>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Inventory mappings
        CreateMap<InventoryEntity, InventoryDto>()
        .ConstructUsing(src => new InventoryDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.Warehouse != null ? src.Warehouse.Code : string.Empty,
        src.Quantity,
        src.ReservedQuantity,
        src.AvailableQuantity,
        src.MinimumStockLevel,
        src.MaximumStockLevel,
        src.UnitCost,
        src.Location,
        src.LastRestockedAt,
        src.LastCountedAt,
        src.CreatedAt
        ));
        CreateMap<CreateInventoryDto, InventoryEntity>();
        CreateMap<UpdateInventoryDto, InventoryEntity>();

        // LowStockAlert mappings
        CreateMap<InventoryEntity, LowStockAlertDto>()
        .ConstructUsing(src => new LowStockAlertDto(
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.Quantity,
        src.MinimumStockLevel,
        src.MinimumStockLevel - src.Quantity
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // StockMovement mappings
        CreateMap<StockMovement, StockMovementDto>()
        .ConstructUsing(src => new StockMovementDto(
        src.Id,
        src.InventoryId,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.MovementType,
        src.MovementType.ToString(),
        src.Quantity,
        src.QuantityBefore,
        src.QuantityAfter,
        src.ReferenceNumber,
        src.ReferenceId,
        src.Notes,
        src.PerformedBy,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null,
        src.FromWarehouseId,
        src.FromWarehouse != null ? src.FromWarehouse.Name : null,
        src.ToWarehouseId,
        src.ToWarehouse != null ? src.ToWarehouse.Name : null,
        src.CreatedAt
        ));
        CreateMap<CreateStockMovementDto, StockMovement>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // DeliveryTimeEstimation mappings
        CreateMap<DeliveryTimeEstimation, DeliveryTimeEstimationDto>()
        .ConstructUsing(src => new DeliveryTimeEstimationDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : null,
        src.CategoryId,
        src.Category != null ? src.Category.Name : null,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : null,
        src.ShippingProviderId,
        src.City,
        src.Country,
        src.MinDays,
        src.MaxDays,
        src.AverageDays,
        src.IsActive,
        !string.IsNullOrEmpty(src.Conditions)
        ? JsonSerializer.Deserialize<DeliveryTimeSettingsDto>(src.Conditions!, (JsonSerializerOptions?)null)
        : null,
        src.CreatedAt
        ));
        CreateMap<CreateDeliveryTimeEstimationDto, DeliveryTimeEstimation>();
        CreateMap<UpdateDeliveryTimeEstimationDto, DeliveryTimeEstimation>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PickPack mappings
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        CreateMap<PickPack, PickPackDto>()
        .ConstructUsing(src => new PickPackDto(
        src.Id,
        src.OrderId,
        src.Order != null ? src.Order.OrderNumber : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.PackNumber,
        src.Status, // ✅ Enum direkt kullanılıyor (ToString() YASAK)
        src.PickedByUserId,
        src.PickedBy != null ? $"{src.PickedBy.FirstName} {src.PickedBy.LastName}" : null,
        src.PackedByUserId,
        src.PackedBy != null ? $"{src.PackedBy.FirstName} {src.PackedBy.LastName}" : null,
        src.PickedAt,
        src.PackedAt,
        src.ShippedAt,
        src.Notes,
        src.Weight,
        src.Dimensions,
        src.PackageCount,
        src.Items != null ? src.Items.Select(i => new PickPackItemDto(
        i.Id,
        i.OrderItemId,
        i.ProductId,
        i.Product != null ? i.Product.Name : string.Empty,
        i.Product != null ? i.Product.SKU : string.Empty,
        i.Quantity,
        i.IsPicked,
        i.IsPacked,
        i.PickedAt,
        i.PackedAt,
        i.Location
        )).ToList().AsReadOnly() : new List<PickPackItemDto>().AsReadOnly(),
        src.CreatedAt
        ));
        CreateMap<CreatePickPackDto, PickPack>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PickPackItem mappings
        CreateMap<PickPackItem, PickPackItemDto>()
        .ConstructUsing(src => new PickPackItemDto(
        src.Id,
        src.OrderItemId,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.Quantity,
        src.IsPicked,
        src.IsPacked,
        src.PickedAt,
        src.PackedAt,
        src.Location
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ShippingAddress mappings
        CreateMap<ShippingAddress, ShippingAddressDto>()
        .ConstructUsing(src => new ShippingAddressDto(
        src.Id,
        src.UserId,
        src.Label,
        src.FirstName,
        src.LastName,
        src.Phone,
        src.AddressLine1,
        src.AddressLine2,
        src.City,
        src.State,
        src.PostalCode,
        src.Country,
        src.IsDefault,
        src.IsActive,
        src.Instructions,
        src.CreatedAt
        ));
        CreateMap<CreateShippingAddressDto, ShippingAddress>();
        CreateMap<UpdateShippingAddressDto, ShippingAddress>();

    }
}
