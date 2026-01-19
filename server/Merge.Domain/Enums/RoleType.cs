namespace Merge.Domain.Enums;

/// <summary>
/// Role context type - Rolün hangi bağlamda kullanıldığını belirtir
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Platform seviyesi roller (Admin, Customer, Seller)
    /// </summary>
    Platform = 0,
    
    /// <summary>
    /// Store bazlı roller (Store Owner, Store Manager, Store Staff)
    /// </summary>
    Store = 1,
    
    /// <summary>
    /// Organizasyon bazlı roller (Organization Admin, Organization Member)
    /// </summary>
    Organization = 2,
    
    /// <summary>
    /// Satıcı web'inde müşteri rolleri (VIP Customer, Regular Customer)
    /// </summary>
    StoreCustomer = 3
}
