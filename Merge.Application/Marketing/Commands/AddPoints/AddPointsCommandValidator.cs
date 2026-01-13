using FluentValidation;

namespace Merge.Application.Marketing.Commands.AddPoints;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class AddPointsCommandValidator() : AbstractValidator<AddPointsCommand>
{
    public AddPointsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si boş olamaz.");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Puan 0'dan büyük olmalıdır.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("İşlem tipi boş olamaz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz.")
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
