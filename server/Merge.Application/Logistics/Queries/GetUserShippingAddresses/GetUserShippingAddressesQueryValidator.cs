using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetUserShippingAddresses;

public class GetUserShippingAddressesQueryValidator : AbstractValidator<GetUserShippingAddressesQuery>
{
    public GetUserShippingAddressesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

