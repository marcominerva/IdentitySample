using IdentitySample.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentitySample.Authentication.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.ConnectionString).HasMaxLength(4000).IsRequired().IsUnicode(false);
        builder.Property(t => t.StorageConnectionString).HasMaxLength(4000).IsUnicode(false);
        builder.Property(t => t.ContainerName).HasMaxLength(256).IsUnicode(false);
    }
}