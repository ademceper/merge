using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;

public class GetEmailSubscriberByEmailQueryValidator : AbstractValidator<GetEmailSubscriberByEmailQuery>
{
    public GetEmailSubscriberByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
    }
}
