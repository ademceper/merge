using FluentValidation;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.CreatePolicy;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class CreatePolicyCommandValidator : AbstractValidator<CreatePolicyCommand>
{
    public CreatePolicyCommandValidator()
    {
        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("CreatedByUserId gereklidir");

        RuleFor(x => x.PolicyType)
            .NotEmpty().WithMessage("Policy type gereklidir")
            .MaximumLength(100).WithMessage("Policy type en fazla 100 karakter olabilir");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık gereklidir")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik gereklidir")
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır");

        RuleFor(x => x.Version)
            .MaximumLength(20).WithMessage("Version en fazla 20 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.Version));

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language gereklidir")
            .MaximumLength(10).WithMessage("Language en fazla 10 karakter olabilir");

        RuleFor(x => x.ChangeLog)
            .MaximumLength(2000).WithMessage("Change log en fazla 2000 karakter olabilir")
            .When(x => !string.IsNullOrEmpty(x.ChangeLog));

        RuleFor(x => x)
            .Must(x => !x.EffectiveDate.HasValue || !x.ExpiryDate.HasValue || x.EffectiveDate.Value < x.ExpiryDate.Value)
            .WithMessage("Effective date, expiry date'den önce olmalıdır");
    }
}

