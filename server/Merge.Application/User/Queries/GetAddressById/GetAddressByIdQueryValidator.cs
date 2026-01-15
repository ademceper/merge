using FluentValidation;

namespace Merge.Application.User.Queries.GetAddressById;

public class GetAddressByIdQueryValidator : AbstractValidator<GetAddressByIdQuery>
{
    public GetAddressByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");
    }
}
