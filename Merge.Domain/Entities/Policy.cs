namespace Merge.Domain.Entities;

/// <summary>
/// Policy Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Policy : BaseEntity
{
    public string PolicyType { get; set; } = string.Empty; // TermsAndConditions, PrivacyPolicy, RefundPolicy, ShippingPolicy, etc.
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // HTML or Markdown content
    public string Version { get; set; } = "1.0"; // Semantic versioning: 1.0, 1.1, 2.0, etc.
    public bool IsActive { get; set; } = true;
    public bool RequiresAcceptance { get; set; } = true; // Users must accept this policy
    public DateTime? EffectiveDate { get; set; } // When this version becomes effective
    public DateTime? ExpiryDate { get; set; } // When this version expires (optional)
    public Guid? CreatedByUserId { get; set; } // Admin who created/updated
    public string? ChangeLog { get; set; } // What changed in this version
    public string Language { get; set; } = "tr"; // Multi-language support
    
    // Navigation properties
    public User? CreatedBy { get; set; }
    public ICollection<PolicyAcceptance> Acceptances { get; set; } = new List<PolicyAcceptance>();
}

