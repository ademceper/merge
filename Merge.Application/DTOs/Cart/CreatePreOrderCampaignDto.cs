using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Create Pre Order Campaign DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class CreatePreOrderCampaignDto : IValidatableObject
{
    [Required(ErrorMessage = "Kampanya adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }
    
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
    public DateTime StartDate { get; set; }
    
    [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
    public DateTime EndDate { get; set; }
    
    [Required(ErrorMessage = "Beklenen teslimat tarihi zorunludur")]
    public DateTime ExpectedDeliveryDate { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum miktar 0 veya daha büyük olmalıdır.")]
    public int MaxQuantity { get; set; } = 0;
    
    [Range(0, 100, ErrorMessage = "Depozito yüzdesi 0 ile 100 arasında olmalıdır.")]
    public decimal DepositPercentage { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Özel fiyat 0 veya daha büyük olmalıdır.")]
    public decimal SpecialPrice { get; set; } = 0;

    // ✅ BOLUM 4.1: Custom Validation - Date validations
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate < DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Başlangıç tarihi gelecekte olmalıdır",
                new[] { nameof(StartDate) });
        }

        if (EndDate <= StartDate)
        {
            yield return new ValidationResult(
                "Bitiş tarihi başlangıç tarihinden sonra olmalıdır",
                new[] { nameof(EndDate) });
        }

        if (ExpectedDeliveryDate <= EndDate)
        {
            yield return new ValidationResult(
                "Beklenen teslimat tarihi bitiş tarihinden sonra olmalıdır",
                new[] { nameof(ExpectedDeliveryDate) });
        }
    }
}
