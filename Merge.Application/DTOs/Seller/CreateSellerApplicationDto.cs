using System.ComponentModel.DataAnnotations;
using Merge.Domain.Entities;

namespace Merge.Application.DTOs.Seller;

public class CreateSellerApplicationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İşletme adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string BusinessName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string BusinessType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 10, ErrorMessage = "Vergi numarası en az 10, en fazla 50 karakter olmalıdır.")]
    public string TaxNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Adres en az 5, en fazla 500 karakter olmalıdır.")]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string BankName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Hesap numarası en az 5, en fazla 50 karakter olmalıdır.")]
    public string BankAccountNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Hesap sahibi adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string BankAccountHolderName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(34, MinimumLength = 15, ErrorMessage = "IBAN en az 15, en fazla 34 karakter olmalıdır.")]
    public string IBAN { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string BusinessDescription { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string ProductCategories { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Tahmini aylık gelir 0 veya daha büyük olmalıdır.")]
    public decimal EstimatedMonthlyRevenue { get; set; }
}
