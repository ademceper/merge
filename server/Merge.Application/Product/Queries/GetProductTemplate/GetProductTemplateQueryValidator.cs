using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductTemplate;

public class GetProductTemplateQueryValidator : AbstractValidator<GetProductTemplateQuery>
{
    public GetProductTemplateQueryValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Şablon ID boş olamaz.");
    }
}
