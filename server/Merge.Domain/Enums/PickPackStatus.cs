using Merge.Domain.Modules.Inventory;
namespace Merge.Domain.Enums;

/// <summary>
/// Pick and pack status values for PickPack entity
/// </summary>
public enum PickPackStatus
{
    Pending = 0,
    Picking = 1,
    Picked = 2,
    Packing = 3,
    Packed = 4,
    Shipped = 5,
    Cancelled = 6
}
