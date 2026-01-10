using FluentValidation;

namespace Merge.Application.Marketing.Commands.UpdateEmailSubscriber;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateEmailSubscriberCommandValidator : AbstractValidator<UpdateEmailSubscriberCommand>
{
    public UpdateEmailSubscriberCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Kaynak en fazla 100 karakter olabilir.");
    }
}
