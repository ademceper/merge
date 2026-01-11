using FluentValidation;

namespace Merge.Application.Product.Queries.GetProductTemplate;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetProductTemplateQueryValidator : AbstractValidator<GetProductTemplateQuery>
{
    public GetProductTemplateQueryValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Şablon ID boş olamaz.");
    }
}
