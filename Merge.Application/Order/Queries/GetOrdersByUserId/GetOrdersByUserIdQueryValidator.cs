using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetOrdersByUserIdQueryValidator : AbstractValidator<GetOrdersByUserIdQuery>
{
    public GetOrdersByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Sayfa numarası en az 1 olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100) // ✅ CONFIGURATION: MaxPageSize configuration'dan alınacak, validator'da hardcoded kalabilir (validation için)
            .WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");
    }
}
