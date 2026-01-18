using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.ML.Commands.ForecastDemand;

public class ForecastDemandCommandValidator(IOptions<MLSettings> mlSettings) : AbstractValidator<ForecastDemandCommand>
{
    private readonly MLSettings config = mlSettings.Value;

    public ForecastDemandCommandValidator() : this(Options.Create(new MLSettings()))
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ForecastDays)
            .GreaterThan(0).WithMessage("Forecast days must be greater than 0.")
            .LessThanOrEqualTo(config.MaxForecastDays).WithMessage($"Forecast days cannot exceed {config.MaxForecastDays}.");
    }
}
