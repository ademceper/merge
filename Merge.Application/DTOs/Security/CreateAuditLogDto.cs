using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Security;

public class CreateAuditLogDto
{
    public Guid? UserId { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string UserEmail { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public Guid? EntityId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string PrimaryKey { get; set; } = string.Empty;
    
    public string OldValues { get; set; } = string.Empty;
    
    public string NewValues { get; set; } = string.Empty;
    
    public string Changes { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Severity { get; set; } = "Info";
    
    [StringLength(100)]
    public string Module { get; set; } = string.Empty;
    
    public bool IsSuccessful { get; set; } = true;
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    public string? AdditionalData { get; set; }
}
