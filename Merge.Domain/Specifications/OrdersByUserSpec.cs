using Merge.Domain.Entities;
using Merge.Domain.Specifications;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.Specifications;

/// <summary>
/// Example Specification - OrdersByUserSpec
/// BOLUM 7.2: Specification Pattern (ZORUNLU)
/// 
/// Kullanım:
/// var spec = new OrdersByUserSpec(userId, page, pageSize);
/// var orders = await _orderRepository.ListAsync(spec);
/// </summary>
public class OrdersByUserSpec : Specification<Order>
{
    public OrdersByUserSpec(Guid userId, int page = 1, int pageSize = 20)
    {
        // ✅ BOLUM 7.2: Specification Pattern - Criteria (Where clause)
        Criteria = o => o.UserId == userId;

        // ✅ BOLUM 7.2: Specification Pattern - Includes (Eager loading)
        AddInclude(o => o.OrderItems);
        AddInclude(o => o.OrderItems.Select(oi => oi.Product)); // Product navigation property
        AddInclude(o => o.Payment);
        AddInclude(o => o.Address);
        AddInclude(o => o.User);

        // ✅ BOLUM 7.2: Specification Pattern - Ordering
        ApplyOrderByDescending(o => o.CreatedAt);

        // ✅ BOLUM 7.2: Specification Pattern - Pagination
        ApplyPaging((page - 1) * pageSize, pageSize);

        // ✅ BOLUM 6.1: AsNoTracking for read-only queries (default: true)
        // IsNoTracking = true; // Default olarak true
    }
}

