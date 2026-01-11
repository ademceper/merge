using FluentValidation;

namespace Merge.Application.Review.Commands.UpdateReview;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty()
            .WithMessage("Değerlendirme ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Puan 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Başlık zorunludur.")
            .MaximumLength(200)
            .WithMessage("Başlık en fazla 200 karakter olabilir.")
            .MinimumLength(2)
            .WithMessage("Başlık en az 2 karakter olmalıdır.");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Yorum zorunludur.")
            .MaximumLength(5000)
            .WithMessage("Yorum en fazla 5000 karakter olabilir.")
            .MinimumLength(10)
            .WithMessage("Yorum en az 10 karakter olmalıdır.");
    }
}
