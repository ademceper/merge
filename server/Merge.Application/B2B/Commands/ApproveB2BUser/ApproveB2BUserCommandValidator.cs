using FluentValidation;

namespace Merge.Application.B2B.Commands.ApproveB2BUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveB2BUserCommandValidator : AbstractValidator<ApproveB2BUserCommand>
{
    public ApproveB2BUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty().WithMessage("Onaylayan kullanıcı ID boş olamaz");
    }
}

