using FluentValidation;

namespace Merge.Application.Order.Queries.FilterOrders;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class FilterOrdersQueryValidator : AbstractValidator<FilterOrdersQuery>
{
    public FilterOrdersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası en az 1 olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinAmount.HasValue)
            .WithMessage("Minimum tutar negatif olamaz.");

        RuleFor(x => x.MaxAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxAmount.HasValue)
            .WithMessage("Maksimum tutar negatif olamaz.");

        RuleFor(x => x)
            .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount.Value <= x.MaxAmount.Value)
            .WithMessage("Minimum tutar maksimum tutardan büyük olamaz.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Başlangıç tarihi bitiş tarihinden sonra olamaz.");

        RuleFor(x => x.OrderNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.OrderNumber))
            .WithMessage("Sipariş numarası en fazla 100 karakter olabilir.");
    }
}
