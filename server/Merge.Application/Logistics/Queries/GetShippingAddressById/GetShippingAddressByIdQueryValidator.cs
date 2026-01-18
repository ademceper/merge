using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

public class GetShippingAddressByIdQueryValidator : AbstractValidator<GetShippingAddressByIdQuery>
{
    public GetShippingAddressByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adres ID'si zorunludur.");
    }
}

