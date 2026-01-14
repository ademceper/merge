using FluentValidation;

namespace Merge.Application.Content.Commands.CreateBlogComment;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateBlogCommentCommandValidator : AbstractValidator<CreateBlogCommentCommand>
{
    public CreateBlogCommentCommandValidator()
    {
        RuleFor(x => x.BlogPostId)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Yorum içeriği zorunludur.")
            .MaximumLength(2000)
            .WithMessage("Yorum içeriği en fazla 2000 karakter olabilir.")
            .MinimumLength(1)
            .WithMessage("Yorum içeriği en az 1 karakter olmalıdır.");

        // Guest comment validation
        When(x => !x.UserId.HasValue, () =>
        {
            RuleFor(x => x.AuthorName)
                .NotEmpty()
                .WithMessage("Misafir yorumlar için yazar adı zorunludur.")
                .MaximumLength(100)
                .WithMessage("Yazar adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.AuthorEmail)
                .NotEmpty()
                .WithMessage("Misafir yorumlar için e-posta adresi zorunludur.")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(200)
                .WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
        });
    }
}

