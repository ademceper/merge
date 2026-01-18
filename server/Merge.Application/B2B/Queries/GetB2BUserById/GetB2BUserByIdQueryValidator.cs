using FluentValidation;

namespace Merge.Application.B2B.Queries.GetB2BUserById;

public class GetB2BUserByIdQueryValidator : AbstractValidator<GetB2BUserByIdQuery>
{
    public GetB2BUserByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");
    }
}

