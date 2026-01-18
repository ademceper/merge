using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetDefaultShippingAddress;

public class GetDefaultShippingAddressQueryValidator : AbstractValidator<GetDefaultShippingAddressQuery>
{
    public GetDefaultShippingAddressQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");
    }
}

