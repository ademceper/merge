using FluentValidation;

namespace Merge.Application.B2B.Commands.UpdateB2BUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateB2BUserCommandValidator : AbstractValidator<UpdateB2BUserCommand>
{
    public UpdateB2BUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");

        RuleFor(x => x.Dto)
            .NotNull().WithMessage("Güncelleme verisi boş olamaz");
    }
}

