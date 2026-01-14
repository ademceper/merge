using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.ML.Commands.ForecastDemand;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class ForecastDemandCommandValidator : AbstractValidator<ForecastDemandCommand>
{
    public ForecastDemandCommandValidator(IOptions<MLSettings> mlSettings)
    {
        var settings = mlSettings.Value;

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ForecastDays)
            .GreaterThan(0).WithMessage("Forecast days must be greater than 0.")
            .LessThanOrEqualTo(settings.MaxForecastDays).WithMessage($"Forecast days cannot exceed {settings.MaxForecastDays}.");
    }
}
