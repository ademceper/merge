using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetUserShippingAddressesQueryValidator : AbstractValidator<GetUserShippingAddressesQuery>
{
    public GetUserShippingAddressesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

