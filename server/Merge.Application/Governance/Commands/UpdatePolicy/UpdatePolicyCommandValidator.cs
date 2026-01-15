using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.UpdatePolicy;

public class UpdatePolicyCommandValidator : AbstractValidator<UpdatePolicyCommand>
{
    public UpdatePolicyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID gereklidir");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("UpdatedByUserId gereklidir");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Content)
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.Version)
            .MaximumLength(20).WithMessage("Version en fazla 20 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Version));

        RuleFor(x => x.ChangeLog)
            .MaximumLength(2000).WithMessage("Change log en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ChangeLog));

        RuleFor(x => x)
            .Must(x => !x.EffectiveDate.HasValue || !x.ExpiryDate.HasValue || x.EffectiveDate.Value < x.ExpiryDate.Value)
            .WithMessage("Effective date, expiry date'den önce olmalıdır")
            .When(x => x.EffectiveDate.HasValue && x.ExpiryDate.HasValue);
    }
}

