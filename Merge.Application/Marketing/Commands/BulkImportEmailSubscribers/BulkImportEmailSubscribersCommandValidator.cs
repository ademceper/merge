using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Commands.BulkImportEmailSubscribers;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class BulkImportEmailSubscribersCommandValidator() : AbstractValidator<BulkImportEmailSubscribersCommand>
{
    public BulkImportEmailSubscribersCommandValidator()
    {
        RuleFor(x => x.Subscribers)
            .NotNull().WithMessage("Subscribers listesi zorunludur.")
            .NotEmpty().WithMessage("En az bir subscriber gerekli.")
            .Must(subscribers => subscribers.Count <= 1000)
            .WithMessage("Bir seferde en fazla 1000 subscriber import edilebilir.");

        RuleForEach(x => x.Subscribers)
            .ChildRules(subscriber =>
            {
                subscriber.RuleFor(s => s.Email)
                    .NotEmpty().WithMessage("E-posta adresi zorunludur.")
                    .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                    .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
            });
    }
}
