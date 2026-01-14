using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetDefaultShippingAddressQueryValidator : AbstractValidator<GetDefaultShippingAddressQuery>
{
    public GetDefaultShippingAddressQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

