using FluentValidation;

namespace Merge.Application.User.Queries.GetAddressById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetAddressByIdQueryValidator : AbstractValidator<GetAddressByIdQuery>
{
    public GetAddressByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");
    }
}
