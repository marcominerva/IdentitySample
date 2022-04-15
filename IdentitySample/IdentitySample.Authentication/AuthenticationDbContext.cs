using IdentitySample.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySample.Authentication;

public class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public DbSet<Tenant> Tenants { get; set; }

    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(u => u.FirstName).HasMaxLength(256).IsRequired();
            user.Property(u => u.LastName).HasMaxLength(256);
        });

        builder.Entity<ApplicationUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).IsRequired();
        });

        builder.Entity<Tenant>(tenant =>
        {
            tenant.ToTable("Tenants");
            tenant.HasKey(t => t.Id);
            tenant.Property(t => t.Id).ValueGeneratedOnAdd();

            tenant.Property(t => t.ConnectionString).HasMaxLength(4000).IsRequired().IsUnicode(false);
            tenant.Property(t => t.StorageConnectionString).HasMaxLength(4000).IsUnicode(false);
            tenant.Property(t => t.ContainerName).HasMaxLength(256).IsUnicode(false);
        });
    }
}
