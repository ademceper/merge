using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductBundleById;

public class GetProductBundleByIdQueryValidator : AbstractValidator<GetProductBundleByIdQuery>
{
    public GetProductBundleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Paket ID boş olamaz.");
    }
}
