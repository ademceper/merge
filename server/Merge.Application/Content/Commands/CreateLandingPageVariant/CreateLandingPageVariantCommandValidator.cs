using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.CreateLandingPageVariant;

public class CreateLandingPageVariantCommandValidator : AbstractValidator<CreateLandingPageVariantCommand>
{
    public CreateLandingPageVariantCommandValidator()
    {
        RuleFor(x => x.OriginalId)
            .NotEmpty().WithMessage("Orijinal landing page ID gereklidir");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("İsim gereklidir")
            .MaximumLength(200).WithMessage("İsim en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("İsim en az 2 karakter olmalıdır");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık gereklidir")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
            .MinimumLength(2).WithMessage("Başlık en az 2 karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik gereklidir")
            .MaximumLength(10000).WithMessage("İçerik en fazla 10000 karakter olabilir")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır");

        RuleFor(x => x.Status)
            .Must(BeValidStatus).WithMessage("Geçersiz durum değeri")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.TrafficSplit)
            .InclusiveBetween(0, 100).WithMessage("Trafik bölünmesi 0 ile 100 arasında olmalıdır");
    }

    private static bool BeValidStatus(string status)
    {
        return Enum.TryParse<ContentStatus>(status, true, out _);
    }
}

