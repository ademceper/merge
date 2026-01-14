using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetShippingAddressById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetShippingAddressByIdQueryValidator : AbstractValidator<GetShippingAddressByIdQuery>
{
    public GetShippingAddressByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adres ID'si zorunludur.");
    }
}

