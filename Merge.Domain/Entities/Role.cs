using Microsoft.AspNetCore.Identity;
using Merge.Domain.Common;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// Role Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// NOT: IdentityRole'dan türüyor, bu yüzden BaseEntity'den türemiyor.
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Role : IdentityRole<Guid>
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation (mümkün olduğunca)
    // NOT: IdentityRole base class'ı bazı property'leri public set gerektiriyor, bu yüzden kısmi private set kullanıyoruz
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ✅ BOLUM 1.1: Domain Method - Update description
    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update name
    public void UpdateName(string name)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
