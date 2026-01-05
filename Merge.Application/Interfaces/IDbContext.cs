using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;
using UserEntity = Merge.Domain.Entities.User;
using RoleEntity = Merge.Domain.Entities.Role;
using UserRole = Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>;

namespace Merge.Application.Interfaces;

// ✅ BOLUM 1.1: Interface'ler Application katmanında olmalı (Clean Architecture)
// ✅ BOLUM 1.2: DbContext dogrudan kullanimi yerine interface kullanilmali
// ⚠️ NOT: User, Role ve UserRole entity'leri BaseEntity'den türemediği için özel property'ler eklendi
public interface IDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;
    DbSet<UserEntity> Users { get; } // ✅ User entity'si IdentityUser'dan türüyor, BaseEntity'den değil
    DbSet<RoleEntity> Roles { get; } // ✅ Role entity'si IdentityRole'dan türüyor, BaseEntity'den değil
    DbSet<UserRole> UserRoles { get; } // ✅ UserRole entity'si IdentityUserRole'dan türüyor, BaseEntity'den değil
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
}

