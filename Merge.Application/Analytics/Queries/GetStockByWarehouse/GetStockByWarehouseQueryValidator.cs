using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetStockByWarehouse;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetStockByWarehouseQueryValidator : AbstractValidator<GetStockByWarehouseQuery>
{
    public GetStockByWarehouseQueryValidator()
    {
        // GetStockByWarehouseQuery parametre almadığı için validation gerekmez
        // Ancak FluentValidation pattern'i için boş validator oluşturuyoruz
    }
}

